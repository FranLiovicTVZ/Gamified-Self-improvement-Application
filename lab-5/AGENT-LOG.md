# Lab 5 - Agent Activity Log

## Datum: 3. lipnja 2026.

---

## Pregled sesije

Ova sesija pokriva kompletnu Lab 5 implementaciju i debugiranje na postojećoj aplikaciji Gamified Self Improvement. Korisnik je imao već neke dijelove napravljene, ali s nekoliko kritičnih bugova i nedostajućim funkcionalnostima.

---

## Faza 1: Analiza i dijagnoza bugova

**Korisnikov zahtjev:** "Provjeri jesu li ispunjeni svi zahtjevi iz lab5.md. Za sada se pri registraciji novog korisnika pojavljuje blank screen od bugova koje sam primjetio."

### Pronađeni bugovi

#### Bug 1: Krivi login path (KRITIČNI — uzrok blank screena)
**Problem:** `AddIdentity<AppUser, IdentityRole>()` po defaultu postavlja `LoginPath = "/Account/Login"`. Kada neautentificirani korisnik pokušava pristupiti zaštićenoj stranici, middleware preusmjerava na `/Account/Login` koji ne postoji → blank screen ili 404.

**Fix:** `Program.cs` — dodano `ConfigureApplicationCookie`:
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/login";
});
```

#### Bug 2: Nedostajao `Properties/launchSettings.json`
**Problem:** Bez `launchSettings.json` app se pokreće u **Production** modu. U Production modu `UseExceptionHandler("/Home/Error")` preusmjerava greške na `/Home/Error` koji nema route u `HomeController` (koji koristi attribute routing) → blank screen umjesto opisa greške.

**Fix:** Kreiran `Properties/launchSettings.json` s `ASPNETCORE_ENVIRONMENT=Development`, port `https://localhost:7000`.

---

## Faza 2: Revizija lab5 zahtjeva

Pregledom koda potvrđeno da su implementirani:
- ✅ API controlleri za sve entitete
- ✅ DTO klase
- ✅ Identity autentikacija
- ✅ AppUser s OIB, JMBG
- ✅ Autorizacijska pravila (Admin, Manager, User)
- ✅ Dropzone upload na Edit formi aktivnosti
- ✅ Integracijski testovi (6 testova, svi prolaze)
- ⚠️ Google/Facebook OAuth — kod postoji, ali nema konfiguriranih credentials

---

## Faza 3: Google OAuth setup

**Korisnikov zahtjev:** Postavi Google/Facebook OAuth credentials.

**Akcije:**
1. Dodan `<UserSecretsId>09b4bd1f-909f-4ebf-bf56-52a10e99c995</UserSecretsId>` u `.csproj`
2. Kreiran `%APPDATA%\Microsoft\UserSecrets\...\secrets.json`
3. Korisnik dostavio Google Client ID: `x`
4. Korisnik dostavio Google Client Secret: `x`
5. Oba postavljena putem `dotnet user-secrets set`

**Facebook:** Korisnik odlučio preskočiti — Google je dovoljan za 1 bod iz OAuth zahtjeva.

---

## Faza 4: Autorizacija i UI poboljšanja

**Korisnikov zahtjev:** Korisnik koji nije admin ne može upravljati aktivnostima svih korisnika nego samo vidjeti njihov popis. Napraviti specifičnog admina.

### Promjene napravljene

#### ActivityController — autorizacija gumbi
**Bug:** "Dodaj Vježbu/Meditaciju/Dnevnik" gumbi prikazivali se svim korisnicima u `Index.cshtml`. "Uredi" i "Obriši" gumbi prikazivali se svim korisnicima u `Details.cshtml`.

**Fix:**
- `Views/Activity/Index.cshtml` — Dodaj gumbi vidljivi samo `Admin` i `Manager`
- `Views/Activity/Details.cshtml` — "Uredi" vidljiv Admin/Manager, "Obriši" samo Admin

#### AdminDashboard poboljšanje
Dodani brzi linkovi za upravljanje aktivnostima direktno s admin dashboarda:
```html
<a asp-controller="Activity" asp-action="Index">⚡ Upravljaj aktivnostima</a>
<a asp-controller="Activity" asp-action="CreateExercise">💪 Nova vježba</a>
```

#### Admin seed korisnici
Dva admin korisnika seedaju se pri pokretanju:
- `admin@gamified.hr` / `Admin123`
- `admin@gmail.com` / `admin`

---

## Faza 5: Google OAuth redirect_uri_mismatch

**Problem:** Korisnik dobio `Error 400: redirect_uri_mismatch` pri Google prijavi.

**Uzrok:** Aplikacija šalje redirect URI `https://localhost:7000/signin-google`, ali Google Console nije imao taj URI.

**Fix:** Korisnik mora dodati `https://localhost:7000/signin-google` u Google Cloud Console → OAuth klijent → Authorized redirect URIs.

---

## Faza 6: Šire autorizacijske promjene

**Korisnikov zahtjev:**
1. Korisnik sam sebi može zapisivati aktivnosti
2. Korisnik koji nije admin ne može dodavati nove korisnike
3. Korisnik vidi samo svoje aktivnosti

### UserController — zaštita

Dodano `[Authorize(Roles = "Admin")]` na:
- `Create` GET + POST — jedino admin može dodavati legacy korisnike
- `Edit` GET + POST
- `Delete` GET + POST

Dodan `using Microsoft.AspNetCore.Authorization;`.

Sakriveno "Dodaj Korisnika" u `Views/User/Index.cshtml`:
```html
@if (User.IsInRole("Admin"))
{
    <a asp-action="Create">➕ Dodaj Korisnika</a>
}
```

### ActivityController — korisnici snimaju vlastite aktivnosti

Promijenjeni atributi s `[Authorize(Roles = "Admin,Manager")]` na `[Authorize]` za:
- `CreateExercise` GET + POST
- `CreateMeditation` GET + POST
- `CreateJournal` GET + POST

Dodane tri helper metode:

```csharp
// Vraća legacy User koji odgovara trenutnom AppUser-u
private async Task<User?> GetCurrentLegacyUserAsync()

// Postavlja ViewBag.Users — admin vidi sve, korisnik samo sebe
private async Task SetCreateViewBagAsync()

// Za non-admin korisnike automatski dodjeljuje UserId i AppUserId
private async Task AssignCurrentUserIfNeededAsync(Activity activity)
```

### ActivityController.Index — filtriranje po korisniku

```csharp
else if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
{
    var legacyUser = await GetCurrentLegacyUserAsync();
    var appUserId = CurrentUserId;
    activities = _activityRepository.GetAll()
        .Where(a => a.AppUserId == appUserId ||
                    (legacyUser != null && a.UserId == legacyUser.Id))
        .ToList();
}
```

Admin/Manager vide sve aktivnosti. Anonimni korisnici vide sve (AllowAnonymous). Prijavljeni User vidi samo svoje.

### Create forme — uvjetni prikaz korisnika

Sve tri Create forme (`CreateExercise.cshtml`, `CreateMeditation.cshtml`, `CreateJournal.cshtml`) prikazuju:
- **Admin/Manager:** dropdown za odabir korisnika
- **Regular user:** read-only prikaz svog korisničkog imena + hidden input s userId

```html
@if (ViewBag.IsOwnActivity == true)
{
    <input type="hidden" name="UserId" value="@ViewBag.CurrentLegacyUserId" />
    <input class="form-control" value="@(ViewBag.Users[0].Username)" disabled />
}
else
{
    <select asp-for="UserId" class="form-control" required>...</select>
}
```

### Activity/Index — Create gumbi za sve prijavljene

```html
@if (User.Identity?.IsAuthenticated == true)
{
    <div>
        <a asp-action="CreateExercise">💪 Dodaj Vježbu</a>
        ...
    </div>
}
```

### Lozinka — min duljina smanjena na 5

```csharp
options.Password.RequiredLength = 5;
```

Omogućuje password `"admin"` za seeded admin korisnika.

---

## Faza 7: XP i Streak live ažuriranje

**Korisnikov zahtjev:** "Streak i XP se ne updateaju uživo kada korisnik zapiše aktivnost."

**Problem:** MVC Create akcije ažurirale su samo legacy `User.TotalXP`, ali `AppUser.TotalXP`, `AppUser.Level`, `AppUser.StreakDays` i `Streak` entitet ostajali su nepromijenjeni. Dashboard čita vrijednosti od `AppUser` i `Streak`, ne legacy `User`.

**Fix:** Dodan `UpdateUserProgressAsync(int xpEarned)` u `ActivityController`:

```csharp
private async Task UpdateUserProgressAsync(int xpEarned)
{
    var appUser = await UserManager.GetUserAsync(User);
    if (appUser == null) return;

    appUser.TotalXP += xpEarned;
    appUser.Level = Math.Min(100, appUser.TotalXP / 100 + 1);
    appUser.LastActiveDate = DateTime.UtcNow;

    var streak = await _dbContext.Streaks.FirstOrDefaultAsync(s => s.UserId == appUser.Id);
    if (streak == null)
    {
        streak = new Streak(appUser.Id);
        _dbContext.Streaks.Add(streak);
    }
    streak.CheckAndResetStreak();
    streak.RecordActivity();
    appUser.StreakDays = streak.CurrentStreak;

    await UserManager.UpdateAsync(appUser);
    await _dbContext.SaveChangesAsync();
}
```

Poziva se na kraju `CreateExercise`, `CreateMeditation`, `CreateJournal` POST akcija.

**Level formula:** `Level = Math.Min(100, TotalXP / 100 + 1)` — svaka 100 XP = 1 level, max 100.

---

## Popis svih izmijenjenih datoteka

| Datoteka | Promjena |
|---|---|
| `Program.cs` | ConfigureApplicationCookie, password min length 5, admin@gmail.com seed |
| `Properties/launchSettings.json` | Kreiran — Development okruženje, port 7000 |
| `Gamified Self Improvement.csproj` | Dodan UserSecretsId |
| `Controllers/ActivityController.cs` | Index filtriranje, Create za sve, helper metode, UpdateUserProgressAsync |
| `Controllers/UserController.cs` | Authorize(Admin) na Create/Edit/Delete |
| `Views/Activity/Index.cshtml` | Create gumbi za sve authenticated |
| `Views/Activity/Details.cshtml` | Uredi/Obriši samo odgovarajuće role |
| `Views/Activity/CreateExercise.cshtml` | Conditional user selector |
| `Views/Activity/CreateMeditation.cshtml` | Conditional user selector |
| `Views/Activity/CreateJournal.cshtml` | Conditional user selector |
| `Views/User/Index.cshtml` | "Dodaj Korisnika" samo Admin |
| `Views/Home/AdminDashboard.cshtml` | Brzi linkovi za aktivnosti |
| User secrets | Google Client ID + Secret |

---

## Rezultati testova

```
Test Run Successful.
Total tests: 6
     Passed: 6
 Total time: 4.7 Seconds
```

Svi testovi prolaze.
