# Lab 3 - Entity Framework & Routing

## 📋 Pregled

Lab 3 predstavlja integraciju **Entity Framework Core** u ASP.NET Core MVC projekt. Aplikacija je prebačena s mock repozitorija na EF repozitorije s SQL Server bazom, te su implementirane prilagođene rute za sve controller akcije.

---

## 📁 Struktura Foldara

```
lab-3/
├── AGENT-LOG.md          # Kompletan log AI agenta interakcija
├── chat-history.json     # Strukturirani razgovori u JSON formatu
├── README.md             # Ovaj fajl
└── (ostale datoteke u glavnom projektu)
```

---

## 🎯 Lab 3 Zahtjevi - ✅ SVI ISPUNJENI

### 1. **EF Konfiguracija** ✅
- [x] **Anotacije na modelima**
  - `[Key]` na svim Id svojstvima
  - `[ForeignKey("User")]` na Activity klasi
  - `virtual ICollection<T>` za 1-N relacije
- [x] **DbContext konfiguracija**
  - `GamefiedSelfImprovementDbContext` s 9 DbSet svojstava
  - OnModelCreating() s seed podacima
  - Inheritance configuration (TPH)
- [x] **SQL Server Baza Podataka**
  - Docker kontejner `mcr.microsoft.com/mssql/server:latest`
  - Baza: `gamified_self_improvement`
  - Connection string u `appsettings.json`
  - Port: 1433
- [x] **Dependency Injection**
  - `AddDbContext<GamefiedSelfImprovementDbContext>` s UseSqlServer()
  - `AddScoped<UserRepository>` i `AddScoped<ActivityRepository>`

### 2. **Prebacivanje na EF Repositories** ✅
- [x] **UserRepository.cs** - CRUD operacije
  - GetAll() - uključuje Activities
  - GetById(int id)
  - GetByUsername(string username)
  - Add(User user)
  - Update(User user)
  - Delete(int id)
- [x] **ActivityRepository.cs** - CRUD + filtriranje
  - GetAll()
  - GetById(int id)
  - GetByUserId(int userId)
  - GetByActivityType(ActivityType type)
  - GetAllExercises(), GetAllMeditations(), itd.
  - Add(), Update(), Delete()
- [x] **Ažuranje Controllera**
  - HomeController - koristi EF repositories
  - ActivityController - koristi EF repositories
  - UserController - koristi EF repositories

### 3. **Custom Routing (9 Ruta)** ✅

| Ruta | Controller | Action | Opis |
|------|-----------|--------|------|
| `/` | Home | Dashboard | Početna stranica |
| `/home` | Home | Dashboard | Alternativna početna |
| `/dashboard` | Home | Dashboard | Dashboard akcija |
| `/aktivnosti` | Activity | Index | Sve aktivnosti |
| `/aktivnosti/{id:int}` | Activity | Details | Detalji aktivnosti |
| `/aktivnosti/po-korisniku/{userId:int}` | Activity | Index | Aktivnosti po korisniku |
| `/korisnici` | User | Index | Svi korisnici |
| `/korisnici/{id:int}` | User | Details | Detalji korisnika |
| `/korisnici/profil/{id:int}` | User | Profile | Profil korisnika |

### 4. **semantic-model.md** ✅
Datoteka sadrži:
- Popis svih 9 tablica/entiteta
- Svojstva svakog entiteta s tipovima podataka
- Popis svih svojstava za svaki entitet
- Relacije između tablica (1-N, N-N)
- TPH (Table Per Hierarchy) objašnjenje
- Primjer SQL strukture

### 5. **sitemap.md** ✅
Datoteka sadrži:
- Sve dostupne URL-e u aplikaciji
- Controller i action za svaki URL
- Korišteni view-ovi
- Parametri s primjerima
- Route constraints objašnjenja

### 6. **SKILL.md - EF Skill** ✅
Datoteka sadrži:
- YAML frontmatter s `patterns` ključnim riječima
- Kontekst projekta (verzije, putanja, baza)
- Standardni workflow za EF promjene
- Česti scenariji (dodavanje polja, nove tablice, relacije)
- Primjeri koda za svaki scenario
- Migracija komande

---

## 📊 Entiteti i Relacije

### **9 Tablica u Bazi**

```
Users (1) ──────────────┐
                        ├──── Activities (N)
                        ├──── DailyJournals (N)
                        ├──── Exercises (TPH)
                        ├──── Meditations (TPH)
                        ├──── SpiritualActivities (TPH)
                        └──── DailyJournals (TPH)

SpiritualBooks ────────────── (FK u SpiritualActivity)

XPRewards ────────────────── (Lookup tablica)

TrainingLogs ─────────────── (Tracking tablica)
```

### **Seed Podaci**

#### Korisnici (3)
- **ivan_temuhin** (Id=1) - 850 XP, Level 5
- **marija_fitness** (Id=2) - 420 XP, Level 3
- **petar_spiritual** (Id=3) - 1200 XP, Level 7

#### Aktivnosti (5)
- 2× Exercise (Trčanje, Vježbanje u teretani)
- 2× Meditation (Ujutna, Večernja)
- 1× DailyJournal (Refleksija)

#### Knjige (3)
- Biblija, Kuran, Tora

#### Nagrade (4)
- Početni trening, Meditacija, Čitanje, Refleksija

---

## 🚀 Kako Pokrenuti Aplikaciju

### **1. Pokrenuti Docker i SQL Server**
```powershell
# SQL Server kontejner je vec spremljen, samo ga pokrenite:
docker start gracious_driscoll

# Ili kreirajte novi:
docker run -d -e ACCEPT_EULA=Y -e SA_PASSWORD=YourStrong(!)Password -p 1433:1433 mcr.microsoft.com/mssql/server:latest
```

### **2. Čekati da se SQL Server pokrene**
```powershell
Start-Sleep -Seconds 20
```

### **3. Pokrenuti Aplikaciju**
```powershell
dotnet run
```

### **4. Otvoriti u Pregledniku**
```
http://localhost:5000
```

---

## 📚 Koncepti Objašnjeni

### **Entity Framework (EF)**
- DbContext kao veza s bazom
- DbSet<T> kao tablica
- Anotacije ([Key], [ForeignKey], virtual)
- Migracije (dotnet ef migrations add/update)
- Seed podaci (HasData)
- Eager Loading (.Include) vs Lazy Loading
- TPH nasljeđivanje

### **Routing**
- Attribute routing ([Route])
- Route constraints ({id:int}, {name:alpha})
- Konvencionalni routing (Program.cs)
- Veza s View-ovima

### **Repositories**
- CRUD operacije s EF
- Filtriranje s LINQ
- Dependency Injection
- Async/Await mogućnosti

---

## ✅ Build & Test Status

```
Build: ✅ Succeeded (2.6s)
Aplikacija: ✅ Running on http://localhost:5000
Dashboard: ✅ Accessible
Korisnici: ✅ Accessible
Aktivnosti: ✅ Accessible
```

---

## 📁 Važne Datoteke

| Datoteka | Svrha |
|----------|-------|
| `Models/GamefiedSelfImprovementDbContext.cs` | DbContext s seed podacima |
| `Models/User.cs` | User entitet |
| `Models/Activities.cs` | Activity i sve izvedene klase |
| `Repositories/UserRepository.cs` | EF repozitorij za korisnike |
| `Repositories/ActivityRepository.cs` | EF repozitorij za aktivnosti |
| `Controllers/HomeController.cs` | Dashboard i home rute |
| `Controllers/ActivityController.cs` | Aktivnosti rute |
| `Controllers/UserController.cs` | Korisnici rute |
| `Program.cs` | DI konfiguracija |
| `appsettings.json` | Connection string |
| `semantic-model.md` | DB dokumentacija |
| `sitemap.md` | Routing dokumentacija |
| `SKILL.md` | EF skill za Copilota |

---

## 🔗 Resursi

- [Entity Framework Core Dokumentacija](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Routing](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [Dependency Injection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

---

## 📝 Napomene za Sljedeće Labour

- **Lab 4**: Očekuje se Authentication sa ASP.NET Identity
- **Lab 5**: Mogućnost za CRUD operacije s formama
- **Lab 6**: Mogućnost za API development

---

**Datum Završetka:** 1.5.2026  
**Status:** ✅ ZAVRŠENO - Svi zahtjevi ispunjeni
