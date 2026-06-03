# Lab 5 - API, Auth, Tests - Implementacija

**Predaja: 12.6.2026.**

---

## Bodovanje

| Kriterij | Bodovi | Status |
|---|---|---|
| API podrška za sve entitete (CRUD, DTO) | 2 | ✅ |
| Autentikacija (local accounts) i autorizacija | 1 | ✅ |
| Upload datoteka (Dropzone) | 1 | ✅ |
| 3rd party autentikacija (Google) | 1 | ✅ |
| Integracijski testovi za API endpointe | 2 | ✅ |
| **Ukupno** | **7** | |

---

## 1. API podrška za sve entitete (CRUD, DTO) — 2 boda

### API Controlleri

Svi API controlleri nalaze se u `Controllers/Api/` i nasljeđuju `BaseApiController` koji sadrži zajedničke mapping metode.

#### ExercisesApiController — `/api/exercises`
- `GET /api/exercises` — dohvat svih vježbi, parametri: `search`, `type`, `difficulty`
- `GET /api/exercises/{id}` — dohvat jedne vježbe, vraća 404 ako ne postoji
- `POST /api/exercises` — kreiranje vježbe (zahtijeva autentikaciju)
- `PUT /api/exercises/{id}` — ažuriranje vježbe (korisnik može samo svoju, admin sve)
- `DELETE /api/exercises/{id}` — brisanje vježbe (korisnik može samo svoju, admin sve)
- `GET /api/exercises/user/{userId}` — vježbe specifičnog korisnika

#### MeditationsApiController — `/api/meditations`
- Iste rute kao vježbe, prilagođene za meditacijske parametre
- Filtriranje po `MeditationType` i `DifficultyLevel`

#### DailyJournalsApiController — `/api/journals`
- CRUD s cookie autentikacijom
- Korisnik može pristupiti samo svojim dnevnicima

#### SpiritualBooksApiController — `/api/spiritual-books`
- `GET` je javan (AllowAnonymous)
- `POST`, `PUT`, `DELETE` zahtijevaju `Admin` rolu

#### SpiritualActivitiesApiController — `/api/spiritual-activities`
- Puni CRUD s pretraživanjem
- Include za `Book` navigacijsko svojstvo u odgovorima

#### AttachmentsApiController — `/api/attachments`
- `GET /api/attachments` — lista datoteka, filter po `activityId`
- `POST /api/attachments/upload` — upload datoteke (multipart/form-data)
- `PUT /api/attachments/{id}` — ažuriranje opisa
- `DELETE /api/attachments/{id}` — soft delete + brisanje s diska
- `POST /api/attachments/{id}/download` — preuzimanje datoteke
- Validacija: max 10MB, dozvoljene ekstenzije: `.pdf`, `.doc`, `.docx`, `.txt`, `.jpg`, `.jpeg`, `.png`, `.gif`, `.zip`

#### AuthApiController — `/api/auth`
- `POST /api/auth/register` — registracija s OIB, JMBG, email, lozinka
- `POST /api/auth/login` — prijava, cookie-based
- `POST /api/auth/logout` — odjava
- `GET /api/auth/me` — podatci trenutnog korisnika
- `PUT /api/auth/profile` — ažuriranje profila
- `POST /api/auth/change-password` — promjena lozinke

#### StreaksApiController — `/api/streaks`
- CRUD za streak entitet
- Ažuriranje streaka korisnika

### DTO klase

Nalaze se u `DTOs/` folderu:

| Datoteka | Sadržaj |
|---|---|
| `UserDTO.cs` | `UserDTO`, `CreateUserDTO`, `LoginDTO`, `LoginResponseDTO` |
| `ActivityDTOs.cs` | `ExerciseDTO`, `CreateExerciseDTO`, `MeditationDTO`, `CreateMeditationDTO`, `DailyJournalDTO`, `SpiritualActivityDTO`, `CreateSpiritualActivityDTO` |
| `OtherDTOs.cs` | `SpiritualBookDTO`, `CreateSpiritualBookDTO`, `AttachmentDTO`, `StreakDTO`, `XPRewardDTO` |

### Mapiranje

`BaseApiController` sadrži zaštićene helper metode:
- `MapExerciseToDTO(Exercise)` → `ExerciseDTO`
- `MapMeditationToDTO(Meditation)` → `MeditationDTO`
- `MapDailyJournalToDTO(DailyJournal)` → `DailyJournalDTO`
- `MapSpiritualActivityToDTO(SpiritualActivity)` → `SpiritualActivityDTO`
- `MapAttachmentToDTO(Attachment)` → `AttachmentDTO`
- `MapUserToDTO(AppUser)` → `UserDTO`

Entiteti se nikada ne izlažu direktno — uvijek kroz DTO.

---

## 2. Autentikacija i autorizacija — 1 bod

### ASP.NET Core Identity konfiguracija

```csharp
// Program.cs
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 5;
    })
    .AddEntityFrameworkStores<GamefiedSelfImprovementDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/login";
});
```

### AppUser proširena klasa

`Models/AppUser.cs` nasljeđuje `IdentityUser` i dodaje:

```csharp
public class AppUser : IdentityUser
{
    [Required] [StringLength(11, MinimumLength = 11)]
    public string OIB { get; set; }         // HR porezni broj

    [Required] [StringLength(13, MinimumLength = 13)]
    public string JMBG { get; set; }        // Matični broj

    public int TotalXP { get; set; }
    public int Level { get; set; }
    public string Bio { get; set; }
    public MeditationType PreferredMeditationType { get; set; }
    public List<int> FavoriteBooks { get; set; }
    public int StreakDays { get; set; }
    public DateTime LastActiveDate { get; set; }
    // + navigacijska svojstva
}
```

### AuthController (MVC)

`Controllers/AuthController.cs` — lokalna registracija i prijava:

- `GET /auth/register` — forma za registraciju
- `POST /auth/register` — validira model, kreira `AppUser`, dodjeljuje "User" rolu, sinkronizira legacy korisnika, prijavljuje korisnika
- `GET /auth/login` — forma za prijavu
- `POST /auth/login` — `PasswordSignInAsync` s lockout podrškom
- `POST /auth/logout` — odjava
- `GET /auth/external-login` — OAuth redirect
- `GET /auth/external-login-callback` — OAuth callback
- `POST /auth/external-login-confirm` — potvrda eksternalne prijave (OIB, JMBG)

### Autorizacijska pravila

| Controller | Akcija | Pristup |
|---|---|---|
| ActivityController | Index | AllowAnonymous |
| ActivityController | Details | Authorize (bilo koji korisnik) |
| ActivityController | Create* | Authorize (svi prijavljeni) |
| ActivityController | Edit* | Authorize(Roles = "Admin,Manager") |
| ActivityController | Delete* | Authorize(Roles = "Admin") |
| UserController | Index, Details | Javno |
| UserController | Create, Edit, Delete | Authorize(Roles = "Admin") |
| HomeController | UserDashboard | Authorize |
| HomeController | AdminDashboard | Authorize(Roles = "Admin") |

### Role

Tri role se seedaju pri pokretanju aplikacije:
- `Admin` — puni pristup svemu
- `Manager` — može kreirati i uređivati aktivnosti
- `User` — može gledati listu + kreirati vlastite aktivnosti

### Admin korisnici (seed)

| Email | Lozinka | Rola |
|---|---|---|
| `admin@gmail.com` | `admin` | Admin |
| `admin@gamified.hr` | `Admin123` | Admin |

### Autorizacija u pogledima (UI)

Gumbi za Create/Edit/Delete se prikazuju samo korisnicima s odgovarajućim rolama:

```html
@if (User.IsInRole("Admin") || User.IsInRole("Manager"))
{
    <a asp-action="Edit">✏️ Uredi</a>
}
@if (User.IsInRole("Admin"))
{
    <a asp-action="Delete">🗑️ Obriši</a>
}
```

### Korisnik vidi samo svoje aktivnosti

Kada prijavljeni korisnik (rola User) posjeti `/aktivnosti`, vidi samo svoje:

```csharp
activities = _activityRepository.GetAll()
    .Where(a => a.AppUserId == appUserId ||
                (legacyUser != null && a.UserId == legacyUser.Id))
    .ToList();
```

Admin i Manager vide sve aktivnosti svih korisnika.

### XP i Streak ažuriranje

Nakon svake snimljene aktivnosti poziva se `UpdateUserProgressAsync`:

```csharp
appUser.TotalXP += xpEarned;
appUser.Level = Math.Min(100, appUser.TotalXP / 100 + 1);
streak.RecordActivity();
appUser.StreakDays = streak.CurrentStreak;
await UserManager.UpdateAsync(appUser);
```

Dashboard odmah prikazuje ažurirane vrijednosti XP, Level i Streak.

---

## 3. Upload datoteka (Dropzone) — 1 bod

### Dropzone na Edit formi

`Views/Activity/Edit.cshtml` sadrži Dropzone komponentu vezanu uz aktivnost:

```html
<form id="attachmentDz"
      asp-controller="Activity"
      asp-action="UploadAttachment"
      asp-route-activityId="@Model.Id"
      enctype="multipart/form-data"
      class="dropzone">
</form>
<div id="attachmentList"></div>
```

Dropzone se konfigurira u `@section Scripts`:

```javascript
Dropzone.options.attachmentDz = {
    maxFilesize: 10,
    acceptedFiles: '.pdf,.doc,.docx,.txt,.jpg,.jpeg,.png,.gif,.zip',
    success: function () { loadAttachments(); }
};

function loadAttachments() {
    $("#attachmentList").load("@Url.Action("GetAttachments", "Activity", new { activityId = Model.Id })");
}
```

### Upload akcija (MVC)

`ActivityController.UploadAttachment`:
1. Validira veličinu (max 10MB) i ekstenziju
2. Sprema datoteku na disk: `wwwroot/uploads/activities/{activityId}/{guid}{ext}`
3. Sprema `Attachment` entitet u bazu s metapodacima
4. Vraća JSON s uspjehom

### Prikaz i brisanje (AJAX)

- `GetAttachments` vraća partial view `_AttachmentList.cshtml` s popisom datoteka
- `DeleteAttachment` briše fizičku datoteku i uklanja zapis iz baze
- JavaScript poziva `loadAttachments()` nakon svakog uspješnog uploada i brisanja

### Attachment model

```csharp
public class Attachment
{
    public int Id { get; set; }
    public int? ActivityId { get; set; }    // veza uz aktivnost
    public string? UserId { get; set; }    // veza uz korisnika
    public string FileName { get; set; }   // originalni naziv
    public string FilePath { get; set; }   // putanja na disku
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
    public bool IsDeleted { get; set; }    // soft delete
}
```

---

## 4. 3rd party autentikacija — 1 bod

### Google OAuth konfiguracija

Credentials se čuvaju u user secrets (nikad u git):

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "..."
dotnet user-secrets set "Authentication:Google:ClientSecret" "..."
```

`Program.cs` kondicionalno dodaje Google provider:

```csharp
if (IsConfigured(googleClientId) && IsConfigured(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.CallbackPath = "/signin-google";
    });
}
```

Redirect URI za Google Cloud Console: `https://localhost:7000/signin-google`

### OAuth flow

1. Korisnik klikne "Prijava s Google računom" na `/auth/login`
2. `ExternalLogin` šalje na Google OAuth consent screen
3. Google vraća authorization code na `/auth/external-login-callback`
4. Ako korisnik postoji → prijava
5. Ako je novi korisnik → forma `ExternalLoginConfirm` za unos OIB i JMBG
6. `ExternalLoginConfirm` kreira `AppUser`, dodjeljuje rolu "User", prijavljuje

Gumb se prikazuje samo ako su credentials konfigurirani:

```csharp
private bool IsProviderConfigured(string provider)
{
    var value = _configuration["Authentication:Google:ClientId"];
    return !string.IsNullOrWhiteSpace(value) &&
           !value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
}
```

---

## 5. Integracijski testovi — 2 boda

### Test projekt

`Gamified.SelfImprovement.Tests/ApiIntegrationTests.cs`

### Infrastruktura

```csharp
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    // WebApplicationFactory pokreće cijelu aplikaciju u memoriji
    // InMemory baza za svaki test — nema ovisnosti o SQL Serveru
    // Roles i admin se seedaju automatski
}
```

`Program.cs` prepoznaje `Testing` okruženje i koristi InMemory bazu:

```csharp
if (builder.Environment.IsEnvironment("Testing"))
{
    services.AddDbContext<...>(opt => opt.UseInMemoryDatabase("GamifiedSelfImprovementTests"));
}
```

### Pokriveni testovi

| Test | Scenariji |
|---|---|
| `AuthRegister_CreatesIdentityAndLegacyUser` | Uspješna registracija, korisnik u bazi |
| `ExercisesCrud_CoversSuccessNotFoundAndValidation` | POST (valid+invalid), GET, GET 404, PUT, DELETE |
| `MeditationsCrud_CoversSuccessNotFoundAndValidation` | Isti obrazac |
| `DailyJournalsCrud_CoversSuccessNotFoundAndValidation` | Isti obrazac |
| `SpiritualBooksCrud_CoversSuccessNotFoundAndValidation` | Admin login + puni CRUD |
| `SpiritualActivitiesCrud_CoversSuccessNotFoundAndValidation` | Isti obrazac |

**Ukupno 6 testova — svi prolaze.**

### Primjer testa

```csharp
[Fact]
public async Task ExercisesCrud_CoversSuccessNotFoundAndValidation()
{
    using var client = _factory.CreateClient(...);
    await RegisterAsync(client);

    // Validacijska greška
    var invalid = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseDTO
        { Title = "x", DurationMinutes = 0 });
    Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

    // Uspješno kreiranje
    var create = await client.PostAsJsonAsync("/api/exercises", new CreateExerciseDTO
        { Title = "Integracijski trening", DurationMinutes = 30, ... });
    Assert.Equal(HttpStatusCode.Created, create.StatusCode);

    // 404 za nepostojeći
    var missing = await client.GetAsync("/api/exercises/999999");
    Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

    // Brisanje
    var delete = await client.DeleteAsync($"/api/exercises/{created.Id}");
    Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
}
```

### Pokretanje testova

```bash
dotnet test Gamified.SelfImprovement.Tests/
```

---

## Struktura datoteka

```
Controllers/
├── Api/
│   ├── BaseApiController.cs          ← mapping metode, streak update
│   ├── ExercisesApiController.cs
│   ├── MeditationsApiController.cs
│   ├── DailyJournalsApiController.cs
│   ├── SpiritualBooksApiController.cs
│   ├── SpiritualActivitiesApiController.cs
│   ├── AttachmentsApiController.cs
│   ├── StreaksApiController.cs
│   └── AuthApiController.cs
├── AuthController.cs                 ← MVC login/register/OAuth
├── ActivityController.cs             ← CRUD + upload + streak update
├── HomeController.cs                 ← admin/user dashboard
├── ProfileController.cs              ← profil korisnika
├── BaseController.cs                 ← UserManager pristup
└── UserController.cs                 ← admin upravljanje korisnicima

DTOs/
├── UserDTO.cs
├── ActivityDTOs.cs
└── OtherDTOs.cs

Models/
├── AppUser.cs                        ← Identity + OIB, JMBG, XP, Level...
├── Attachment.cs
├── Streak.cs
└── Activities.cs

Services/
└── UserSyncService.cs                ← sinkronizacija AppUser → legacy User

Views/
├── Auth/
│   ├── Login.cshtml
│   ├── Register.cshtml
│   └── ExternalLoginConfirm.cshtml
├── Home/
│   ├── UserDashboard.cshtml          ← XP, Level, Streak, aktivnosti
│   └── AdminDashboard.cshtml         ← sve statistike + brzi linkovi
└── Activity/
    ├── Edit.cshtml                   ← Dropzone upload
    └── (Create*.cshtml)              ← conditional user selector

Gamified.SelfImprovement.Tests/
└── ApiIntegrationTests.cs            ← 6 integracijskih testova

Properties/
└── launchSettings.json               ← Development okruženje, port 7000
```

---

## Pokretanje aplikacije

```bash
# Baza (Docker)
docker-compose up -d

# Aplikacija
dotnet run

# Testovi
dotnet test Gamified.SelfImprovement.Tests/
```

App dostupna na: `https://localhost:7000`
