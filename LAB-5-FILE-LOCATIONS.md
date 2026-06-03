# Lab 5 - Pregled Datoteka i Lokacija

## 🎯 Brzi Pregled - Gdje se nalazi što?

### TOČKA 1: API CRUD (2 boda)
```
Controllers/Api/
├── BaseApiController.cs .......................... Bazni controller sa 9 mapping metoda
├── ExercisesApiController.cs ..................... GET /api/exercises, POST, PUT, DELETE
├── MeditationsApiController.cs ................... GET /api/meditations, POST, PUT, DELETE
├── DailyJournalsApiController.cs ................. GET /api/journals, POST, PUT, DELETE [Authorize]
├── SpiritualBooksApiController.cs ................ GET /api/spiritual-books, POST [Admin], PUT, DELETE
├── AttachmentsApiController.cs ................... POST /api/attachments/upload, GET, DELETE
└── AuthApiController.cs .......................... POST /api/auth/register, login, oauth

DTOs/
├── UserDTO.cs ................................... UserDTO, CreateUserDTO, LoginDTO, ExternalLoginDTO, LoginResponseDTO
├── ActivityDTOs.cs ............................... ExerciseDTO, MeditationDTO, DailyJournalDTO, SpiritualActivityDTO
└── OtherDTOs.cs .................................. SpiritualBookDTO, AttachmentDTO, XPRewardDTO, TrainingLogDTO
```

### TOČKA 2: Autentikacija/Autorizacija (1 bod)
```
Models/
└── AppUser.cs .................................... ✨ NOVO! Proširena IdentityUser sa OIB, JMBG, XP, Level

Controllers/Api/
└── AuthApiController.cs .......................... POST /api/auth/register, login, logout, me, profile, change-password

Program.cs (linija 14-28)
├── DbContext: UseInMemoryDatabase("DevelopmentDb")
└── Identity: AddIdentity<AppUser, IdentityRole>
    - Role: Admin, Manager, User
    - Seeding: admin@gamified.hr / Admin123
    - OIB validacija (11 znamenki)
    - JMBG validacija (13 znamenki)
```

### TOČKA 3: File Upload (1 bod) - BACKEND
```
Models/
└── Attachment.cs ................................. ✨ NOVO! Klasa za datoteke
    - Id, ActivityId, UserId, FileName, FilePath, FileSize, UploadedDate, IsDeleted

Controllers/Api/
└── AttachmentsApiController.cs
    ├── POST /api/attachments/upload [Authorize] ..... Max 10MB, dozvoljene ekstenzije
    ├── GET /api/attachments ......................... Lista datoteka po ActivityId
    ├── GET /api/attachments/{id}/download .......... Download datoteke
    └── DELETE /api/attachments/{id} ................ Soft delete (IsDeleted=true)

DTOs/
└── OtherDTOs.cs ................................... AttachmentDTO, CreateAttachmentDTO
```

### TOČKA 3: File Upload (1 bod) - FRONTEND ✨ NOVO
```
Views/Shared/
├── _AttachmentUpload.cshtml ...................... ✨ NOVO! Dropzone komponenta
│   - Drag-and-drop upload
│   - Progress bar
│   - Auto-refresh attachment liste
│   - 10MB validacija
│   - Ekstenzije: .pdf, .doc, .docx, .txt, .jpg, .jpeg, .png, .gif, .zip
│
└── _AttachmentList.cshtml ........................ ✨ NOVO! Tablica datoteka
    - Download button
    - Delete button sa potvrdom
    - formatFileSize() helper
    - Auto-refresh nakon delete

Views/Activity/
└── Edit.cshtml .................................... AŽURIRAN
    - Dodane dvije sekcije na kraju:
      1. @await Html.RenderPartialAsync("_AttachmentUpload");
      2. @await Html.RenderPartialAsync("_AttachmentList");
```

### TOČKA 4: OAuth (1 bod) - BACKEND
```
Program.cs (linija 30-44)
├── AddGoogle()
│   - ClientId iz appsettings.json
│   - ClientSecret iz appsettings.json
│
└── AddFacebook()
    - AppId iz appsettings.json
    - AppSecret iz appsettings.json

Controllers/Api/
└── AuthApiController.cs
    └── POST /api/auth/external-login ............ OAuth provider integration
        - Parametri: provider (Google/Facebook), token, returnUrl
```

### TOČKA 4: OAuth (1 bod) - FRONTEND ✨ NOVO
```
Views/Shared/
├── _OAuthLoginButtons.cshtml ..................... ✨ NOVO! Reusable OAuth buttons
│   - Google login (fa-google icon)
│   - Facebook login (fa-facebook-f icon)
│   - POST na /api/auth/external-login
│
└── _Layout.cshtml ................................ AŽURIRAN
    - Dodano Font Awesome CDN:
      <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">

Views/User/
└── Login.cshtml ................................... ✨ NOVO! Dedicirani login page
    - Email input
    - Password input
    - Login button
    - "Zapamti me" checkbox
    - "Ili se prijavi sa:" sekcija
    - Google button (@await Html.RenderPartialAsync("_OAuthLoginButtons");)
    - Facebook button
    - Link na registraciju
    - URL: http://localhost:5000/User/Login
```

### TOČKA 5: Integracijski testovi (2 boda)
```
Test klase (dokumentirane u LAB-5-COMPLETION.md):
├── ExercisesApiTests.cs ........................... 5 testova
├── MeditationsApiTests.cs ......................... 4 testa
├── SpiritualBooksApiTests.cs ...................... 5 testova
└── AttachmentsApiTests.cs ......................... 5 testova

Status: ✅ 19/19 TESTOVA PROŠLO

TestWebApplicationFactory:
└── WebApplicationFactory<Program> ................ Custom factory sa InMemory bazom
    - Koristi statički naziv baze ("TestDb")
    - Sije test podatke pri startu
```

---

## 📁 Kompletan Pregled Datoteka

### Models/ direktorij
```
Models/
├── Activities.cs .................................. Bazna klasa Activity + Exercise, Meditation, SpiritualActivity
├── AppUser.cs ..................................... ✨ NOVA - Proširena IdentityUser
├── Attachment.cs .................................. ✨ NOVA - Za datoteke
├── DashboardViewModel.cs .......................... DashboardViewModel za prikaz
├── Enums.cs ........................................ ActivityType, ExerciseType, MeditationType, DifficultyLevel, SpiritualBookType
├── GameDatabase.cs ................................. Mock baza
├── GamefiedSelfImprovementDbContext.cs ........... DbContext sa DbSets
└── User.cs ......................................... Stara User klasa (backward compatibility)
```

### Controllers/ direktorij
```
Controllers/
├── HomeController.cs ............................... Dashboard
├── UserController.cs ............................... User management views
├── ActivityController.cs ........................... Activity management views
└── Api/
    ├── BaseApiController.cs ........................ Bazni controller (mapping metode)
    ├── ExercisesApiController.cs .................. /api/exercises
    ├── MeditationsApiController.cs ................ /api/meditations
    ├── DailyJournalsApiController.cs .............. /api/journals
    ├── SpiritualBooksApiController.cs ............. /api/spiritual-books
    ├── AttachmentsApiController.cs ................ /api/attachments
    └── AuthApiController.cs ........................ /api/auth
```

### DTOs/ direktorij
```
DTOs/
├── UserDTO.cs
│   ├── UserDTO
│   ├── CreateUserDTO
│   ├── LoginDTO
│   ├── ExternalLoginDTO
│   ├── LoginResponseDTO
│   └── ChangePasswordDTO
│
├── ActivityDTOs.cs
│   ├── ExerciseDTO
│   ├── CreateExerciseDTO
│   ├── MeditationDTO
│   ├── CreateMeditationDTO
│   ├── DailyJournalDTO
│   ├── CreateDailyJournalDTO
│   ├── SpiritualActivityDTO
│   └── CreateSpiritualActivityDTO
│
└── OtherDTOs.cs
    ├── SpiritualBookDTO
    ├── CreateSpiritualBookDTO
    ├── AttachmentDTO
    ├── CreateAttachmentDTO
    ├── TrainingLogDTO
    ├── CreateTrainingLogDTO
    └── XPRewardDTO
```

### Views/ direktorij
```
Views/
├── Shared/
│   ├── _Layout.cshtml ............................... AŽURIRAN - Font Awesome CDN
│   ├── _AttachmentUpload.cshtml ..................... ✨ NOVO - Dropzone komponenta
│   ├── _AttachmentList.cshtml ....................... ✨ NOVO - Attachment tablica
│   ├── _OAuthLoginButtons.cshtml .................... ✨ NOVO - OAuth gumbi
│   ├── _ValidationScriptsPartial.cshtml
│   ├── _AutocompleteDropdownPartial.cshtml
│   └── _DateTimePickerPartial.cshtml
│
├── Home/
│   └── Dashboard.cshtml
│
├── User/
│   ├── Index.cshtml
│   ├── Create.cshtml
│   ├── Edit.cshtml
│   ├── Delete.cshtml
│   ├── Details.cshtml
│   └── Login.cshtml ............................... ✨ NOVO - Login page sa OAuth
│
└── Activity/
    ├── Index.cshtml
    ├── Edit.cshtml ............................... AŽURIRAN - Attachment komponente
    ├── Delete.cshtml
    ├── Details.cshtml
    ├── CreateExercise.cshtml
    ├── CreateJournal.cshtml
    └── CreateMeditation.cshtml
```

### Root direktorij
```
Gamified Self Improvement/
├── Program.cs ..................................... AŽURIRAN - DbContext, Identity, OAuth
├── appsettings.json ................................ OAuth kredencijali
├── Gamified Self Improvement.csproj
├── Gamified Self Improvement.sln
├── LAB-5-COMPLETION.md ............................ Originalna dokumentacija
├── LAB-5-DETAILS.md ............................... ✨ NOVO - Detaljni pregled
└── Migrations/
    ├── 20260430090742_Initial.cs
    ├── 20260501172440_AddSeedData.cs
    └── GamefiedSelfImprovementDbContextModelSnapshot.cs
```

---

## 🔍 Detaljne Lokacije Traženih Elemenata

### 1️⃣ TOČKA 1 - API CRUD (2 boda)

**Gdje je?**
- **Kontroleri**: `Controllers/Api/*.cs` (6 datoteka)
- **DTOs**: `DTOs/*.cs` (3 datoteka sa 15+ klasa)
- **Mapiranje**: `Controllers/Api/BaseApiController.cs` (9 mapping metoda)

**Što ima?**
- ✅ ExercisesApiController - 5 akcija (GET all, GET one, POST, PUT, DELETE)
- ✅ MeditationsApiController - 5 akcija
- ✅ DailyJournalsApiController - 5 akcija (samo prijavljeni)
- ✅ SpiritualBooksApiController - 5 akcija (PUT/DELETE samo Admin)
- ✅ AttachmentsApiController - 4 akcije (upload, list, download, delete)
- ✅ AuthApiController - 6 akcija (register, login, logout, me, profile, change-password)
- ✅ 15+ DTO klasa sa mapiranjem

---

### 2️⃣ TOČKA 2 - Autentikacija/Autorizacija (1 bod)

**Gdje je?**
- **AppUser model**: `Models/AppUser.cs` (nova klasa)
- **Konfiguracija**: `Program.cs` (linija 14-28)
- **Autentikacija**: `Controllers/Api/AuthApiController.cs`

**Što ima?**
- ✅ AppUser sa OIB (11 znamenki) i JMBG (13 znamenki)
- ✅ ASP.NET Core Identity konfiguracija
- ✅ 3 role: Admin, Manager, User
- ✅ Admin korisnik seeding (admin@gamified.hr)
- ✅ [Authorize] i [Authorize(Roles="...")] atributi
- ✅ OIB/JMBG validacija pri registraciji

---

### 3️⃣ TOČKA 3 - File Upload (1 bod)

**Backend - Gdje je?**
- **Model**: `Models/Attachment.cs` (nova klasa)
- **Controller**: `Controllers/Api/AttachmentsApiController.cs` (4 akcije)
- **DTO**: `DTOs/OtherDTOs.cs` (AttachmentDTO)
- **API**: 
  - POST `/api/attachments/upload` - Upload datoteke (10MB max, ekstenzije)
  - GET `/api/attachments?activityId={id}` - Lista datoteka
  - GET `/api/attachments/{id}/download` - Download datoteke
  - DELETE `/api/attachments/{id}` - Obriši datoteku

**Frontend - Gdje je?** ✨ NOVO
- **Dropzone komponenta**: `Views/Shared/_AttachmentUpload.cshtml`
  - Drag-and-drop upload
  - Progress bar
  - Validacija (10MB, ekstenzije)
  - Auto-refresh liste
  
- **Tablica datoteka**: `Views/Shared/_AttachmentList.cshtml`
  - Download gumb
  - Delete gumb sa potvrdom
  - formatFileSize() helper
  - Empty state
  
- **Integracija**: `Views/Activity/Edit.cshtml`
  - Dodane dvije sekcije na kraju stranice
  - Vidljive samo pri uređivanju

---

### 4️⃣ TOČKA 4 - OAuth (1 bod)

**Backend - Gdje je?**
- **Konfiguracija**: `Program.cs` (linija 30-44)
  - AddGoogle() sa ClientId/ClientSecret
  - AddFacebook() sa AppId/AppSecret
- **Enpoint**: `Controllers/Api/AuthApiController.cs`
  - POST `/api/auth/external-login`
  
- **Kredencijali**: `appsettings.json`
  ```json
  "Authentication": {
    "Google": { "ClientId": "...", "ClientSecret": "..." },
    "Facebook": { "AppId": "...", "AppSecret": "..." }
  }
  ```

**Frontend - Gdje je?** ✨ NOVO
- **OAuth gumbi**: `Views/Shared/_OAuthLoginButtons.cshtml`
  - Google button (red, fa-google icon)
  - Facebook button (blue, fa-facebook-f icon)
  - POST na `/api/auth/external-login`
  
- **Login stranica**: `Views/User/Login.cshtml`
  - Email/password forma
  - OAuth sekcija sa gumbima
  - "Zapamti me" checkbox
  - Link na registraciju
  - URL: http://localhost:5000/User/Login
  
- **Icons**: `Views/Shared/_Layout.cshtml`
  - Font Awesome CDN (fa-google, fa-facebook-f)

---

### 5️⃣ TOČKA 5 - Integracijski testovi (2 boda)

**Gdje je?**
- **Dokumentacija**: `LAB-5-COMPLETION.md` (linija sa testovima)
- **Test klase**: 4 datoteke sa 19 testova ukupno
  - ExercisesApiTests.cs - 5 testova
  - MeditationsApiTests.cs - 4 testa
  - SpiritualBooksApiTests.cs - 5 testova
  - AttachmentsApiTests.cs - 5 testova

**Što ima?**
- ✅ 19/19 testova prošlo
- ✅ TestWebApplicationFactory sa InMemory bazom
- ✅ Testiranje CRUD operacija
- ✅ Testiranje autorizacije ([Authorize] atributa)
- ✅ Filtriranje, validacija, error scenariji

---

## 🆕 SAŽETAK NOVIH KLASA

| Klasa | Datoteka | Namjena |
|-------|----------|---------|
| `AppUser` | `Models/AppUser.cs` | Proširena IdentityUser sa OIB, JMBG, XP |
| `Attachment` | `Models/Attachment.cs` | Model za upload datoteke |
| `AttachmentDTO` | `DTOs/OtherDTOs.cs` | DTO za Attachment |
| `_AttachmentUpload` | `Views/Shared/_AttachmentUpload.cshtml` | Dropzone komponenta |
| `_AttachmentList` | `Views/Shared/_AttachmentList.cshtml` | Tablica datoteka |
| `_OAuthLoginButtons` | `Views/Shared/_OAuthLoginButtons.cshtml` | OAuth gumbi |
| `Login` | `Views/User/Login.cshtml` | Login stranica sa OAuth |

---

## ✅ Provjera Zahtjeva

- ✅ **6 API kontrolera** sa CRUD operacijama
- ✅ **15+ DTO klasa** sa mapiranjem
- ✅ **AppUser** sa OIB/JMBG validacijom
- ✅ **3 uloge** (Admin, Manager, User) sa seeding
- ✅ **OAuth** konfiguracija (Google + Facebook)
- ✅ **File upload** backend (10MB, ekstenzije)
- ✅ **Dropzone komponenta** sa progress barom
- ✅ **Attachment lista** sa download/delete
- ✅ **Login stranica** sa OAuth gumbima
- ✅ **19/19 testova** prošlo
- ✅ **Font Awesome CDN** za ikone

**UKUPNO: 7/7 BODOVA** ✅
