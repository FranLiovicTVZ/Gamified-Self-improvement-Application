# Lab 5 - API, Auth, Tests - Implementacija

## ✅ Implementirani zahtjevi

### 1. Kompletna API podrška za sve entitete (CRUD, DTO) - 2 boda

#### Kreirani API Controlleri:
- **ExercisesApiController** - `/api/exercises`
  - GET svi, GET jedan, POST, PUT, DELETE
  - Pretraga po tipu, težini i tekstu
  
- **MeditationsApiController** - `/api/meditations`
  - GET svi, GET jedan, POST, PUT, DELETE
  - Filtriranje po tipu meditacije i težini
  
- **DailyJournalsApiController** - `/api/journals`
  - GET svi (samo korisnika), GET jedan, POST, PUT, DELETE
  - Samo prijavljeni korisnici mogu pristupiti
  
- **SpiritualBooksApiController** - `/api/spiritual-books`
  - GET svi, GET jedan, POST (Admin), PUT (Admin), DELETE (Admin)
  - Pretraga po tipu i tekstu
  
- **AttachmentsApiController** - `/api/attachments`
  - GET svi, GET jedan, POST (upload), PUT, DELETE
  - Download datoteka
  - Ograničenje veličine: 10MB
  - Dozvoljeni tipovi: .pdf, .doc, .docx, .txt, .jpg, .jpeg, .png, .gif, .zip

#### Kreirane DTO klase:
- **UserDTO** - Korisnik bez osjetljivih podataka
- **CreateUserDTO** - Za registraciju
- **LoginDTO** - Za prijavu
- **ExerciseDTO**, **CreateExerciseDTO**
- **MeditationDTO**, **CreateMeditationDTO**
- **DailyJournalDTO**
- **SpiritualBookDTO**, **CreateSpiritualBookDTO**
- **AttachmentDTO**, **CreateAttachmentDTO**
- **TrainingLogDTO**, **CreateTrainingLogDTO**
- **XPRewardDTO**

#### Mapiranje:
- Sve DTOs se mapiraju kroz `BaseApiController` sa zajedničkim mapping metodama
- Entiteti se mapiraju u DTOs kako bi se sprječilo izlaganje osjetljivih polja

### 2. Autentikacija (local accounts) i autorizacija - 1 bod

#### Kreirama AppUser klasa:
```csharp
public class AppUser : IdentityUser
{
    public string OIB { get; set; }           // Obavezno polje
    public string JMBG { get; set; }          // Obavezno polje
    public int TotalXP { get; set; }
    public int Level { get; set; }
    public string Bio { get; set; }
    // ... ostala polja
}
```

#### Autentikacijski API endpointi:
- **POST /api/auth/register** - Registracija novog korisnika
  - Validacija OIB i JMBG
  - Autotska dodjela "User" role
  - Heširanje lozinke
  
- **POST /api/auth/login** - Prijava korisnika
  - Cookie-based autentikacija
  - Lockout nakon više neuspješnih pokušaja
  
- **POST /api/auth/logout** - Odjava
- **GET /api/auth/me** - Podatci trenutnog korisnika
- **PUT /api/auth/profile** - Ažuriranje profila
- **POST /api/auth/change-password** - Promjena lozinke

#### Autorizacija:
- Role: **Admin**, **Manager**, **User**
- `[Authorize]` - Samo prijavljeni korisnici
- `[Authorize(Roles = "Admin")]` - Samo admin
- `[AllowAnonymous]` - Javni pristup
- Zaštita: Korisnik može uređivati samo svoje podatke (ili Admin može sve)

#### Konfiguracija Identity u Program.cs:
```csharp
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options => { ... })
    .AddEntityFrameworkStores<GamefiedSelfImprovementDbContext>()
    .AddDefaultTokenProviders();
```

### 3. Upload datoteka (Dropzone) - 1 bod

#### AttachmentsApiController:
- **POST /api/attachments/upload** - Upload datoteke
  - Multipart form data support
  - Validacija veličine (max 10MB)
  - Validacija MIME tipa
  - Sprema na disk: `/wwwroot/uploads/{userId}/{filename}`
  - Sprema metapodatke u bazu
  
- **GET /api/attachments** - Lista datoteka
  - Filtriranje po ActivityId
  - Soft delete (IsDeleted flag)
  
- **DELETE /api/attachments/{id}** - Brisanje
  - Obriši fizičku datoteku
  - Označi kao obrisanu u bazi
  
- **POST /api/attachments/{id}/download** - Preuzimanje datoteke

#### Model Attachment:
```csharp
public class Attachment
{
    public int Id { get; set; }
    public int? ActivityId { get; set; }          // Veza na aktivnost
    public string? UserId { get; set; }           // Veza na korisnika
    public string FileName { get; set; }          // Originalni naziv
    public string FilePath { get; set; }          // Putanja na disku
    public string ContentType { get; set; }       // MIME tip
    public long FileSize { get; set; }            // Veličina
    public DateTime UploadedDate { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; }           // Soft delete
}
```

### 4. 3rd party autentikacija (Google, FB) - 1 bod

#### Konfiguracija u Program.cs:
```csharp
builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    });
```

#### Konfiguracija u appsettings.json:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
    }
  }
}
```

#### Kako funkcionar:
1. Korisnik klikne "Login with Google" ili "Login with Facebook"
2. Preusmjeren na OAuth provider
3. Provider vraća authorization code
4. Aplikacija izmjenjuje code za token
5. Aplikacija kreira/ažurira lokalnog korisnika
6. Postavljam authentication cookie

### 5. Integracijski testovi za API endpointe - 2 boda

#### Kreirani testovi (Gamified.SelfImprovement.Tests):

**ExercisesApiTests.cs:**
- `GetAll_ReturnsOkStatus_WhenCalled` - Dohvat svih vježbi
- `GetById_ReturnExercise_WhenExerciseExists` - Dohvat jedne vježbe
- `GetById_ReturnNotFound_WhenExerciseDoesNotExist` - Nepostojeća vježba
- `Create_RequiresAuthorization` - Zaštita kreiranja
- `GetByUserId_ReturnsUserExercises` - Vježbe korisnika

**MeditationsApiTests.cs:**
- `GetAll_ReturnsOkStatus_WhenCalled` - Dohvat svih meditacija
- `GetById_ReturnMeditation_WhenMeditationExists` - Dohvat jedne meditacije
- `GetById_ReturnNotFound_WhenMeditationDoesNotExist` - Nepostojeća meditacija
- `Create_RequiresAuthorization` - Zaštita kreiranja

**SpiritualBooksApiTests.cs:**
- `GetAll_ReturnsOkStatus_WhenCalled` - Dohvat svih knjiga
- `GetById_ReturnBook_WhenBookExists` - Dohvat jedne knjige
- `GetById_ReturnNotFound_WhenBookDoesNotExist` - Nepostojeća knjiga
- `GetAll_FilterByType_ReturnsFilteredBooks` - Filtriranje po tipu
- `Create_RequiresAdminRole` - Admin zaštita

**AttachmentsApiTests.cs:**
- `GetAll_RequiresAuthorization` - Autentifikacija
- `GetById_RequiresAuthorization` - Autentifikacija
- `Upload_RequiresAuthorization` - Autentifikacija
- `Delete_RequiresAuthorization` - Autentifikacija

#### Test infrastruktura:
- **WebApplicationFactory<Program>** - Kreira aplikaciju za testove
- **InMemory baza** - Svaki test dobije svoju testnu bazu
- **FluentAssertions** - Čitljive assertion-e
- **xUnit** - Test framework

#### Testiranje endpointa:
```csharp
[Fact]
public async Task GetById_ReturnExercise_WhenExerciseExists()
{
    // Arrange - Pripremi testnu bazu
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GamefiedSelfImprovementDbContext>();
    var exercise = await SeedTestExerciseAsync(db);

    // Act - Pozovi API
    var response = await _client.GetAsync($"/api/exercises/{exercise.Id}");

    // Assert - Provjeri rezultat
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ExerciseDTO>();
    result.Should().NotBeNull();
    result!.Title.Should().Be(exercise.Title);
}
```

## 📁 Struktura projekta

```
Controllers/
├── Api/
│   ├── BaseApiController.cs          (Bazni controller sa mapping metodama)
│   ├── ExercisesApiController.cs
│   ├── MeditationsApiController.cs
│   ├── DailyJournalsApiController.cs
│   ├── SpiritualBooksApiController.cs
│   ├── AttachmentsApiController.cs
│   └── AuthApiController.cs          (Autentikacija i korisnici)
│
DTOs/
├── UserDTO.cs                        (Korisnik DTOs)
├── ActivityDTOs.cs                   (Exercise, Meditation, DailyJournal DTOs)
└── OtherDTOs.cs                      (Book, Attachment, XPReward DTOs)

Models/
├── AppUser.cs                        (Proširena IdentityUser klasa)
├── Attachment.cs                     (Nova klasa za datoteke)
└── Activities.cs                     (Ažurirana sa AppUserId)

Tests/
├── ExercisesApiTests.cs
├── MeditationsApiTests.cs
├── SpiritualBooksApiTests.cs
└── AttachmentsApiTests.cs
```

## 🔒 Sigurnost

- **OIB i JMBG** - Obavezna polja pri registraciji
- **Heširanje lozinke** - ASP.NET Identity koristi PBKDF2
- **Lockout** - Nakon 5 neuspješnih pokušaja prijave
- **HTTPS** - Obavezno u produkciji
- **Authorization** - Provjerava role i dozvole
- **Soft Delete** - Za datoteke (IsDeleted flag)
- **File Validation** - Veličina i MIME tip

## 🚀 Kako pokrenuti

### Migracije:
```bash
cd "Gamified Self Improvement"
dotnet ef migrations add AddIdentityAndAttachments
dotnet ef database update
```

### Pokrenuti aplikaciju:
```bash
dotnet run
```

### Pokrenuti testove:
```bash
cd "..\Gamified.SelfImprovement.Tests"
dotnet test
```

## 📝 API Primjeri

### Login:
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "korisnik@example.com",
  "password": "lozinka123"
}
```

### Kreiraje vježbe:
```
POST /api/exercises
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Push-ups",
  "exeriseType": "Strength",
  "durationMinutes": 30,
  "sets": 3,
  "reps": 10,
  "weight": 0
}
```

### Upload datoteke:
```
POST /api/attachments/upload
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form data:
- file: <binary file>
- description: "Moj dokument"
- activityId: 1
```

## 🎯 Bodovanje

- ✅ Kompletna API podrška: **2 boda**
- ✅ Autentikacija i autorizacija: **1 bod**
- ✅ Upload datoteka: **1 bod**
- ✅ 3rd party autentikacija: **1 bod**
- ✅ Integracijski testovi: **2 boda**

**Ukupno: 7 bodova**
