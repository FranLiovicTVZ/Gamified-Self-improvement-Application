# Lab 2 - AI Agent Interaction Log

**Projekt:** Gamified Self Improvement - HTML/Binding  
**Datum:** 9.4.2026  
**Student:** Fran Liovič  
**AI Agent:** GitHub Copilot (Claude)

---

## 📋 Summary Razgovora

### 1. **Inicijalni Zahtjev - Konverzija u ASP.NET MVC**
- **Tema:** "Ajmo napraviti korake opisane u Lab 2 fileu u kontekstu moje gamified self improvement aplikacije"
- **Odgovor:** Trebam konvertirati konzolu aplikaciju u web aplikaciju s MVC arhitekturom
- **Rezultat:** 
  - ✅ Konvertiran .csproj u SDK.Web
  - ✅ Napravljena folder struktura (Controllers, Views, Models, Repositories)
  - ✅ Kreirian Program.cs za ASP.NET MVC
  - ✅ Dependency injection setup

### 2. **Custom UX/UI Agent Definition (.agent.md)**
- **Tema:** Trebam custom agenta za UI/UX koji će doradi dizajn
- **Odgovor:** Kreirat ću .agent.md s jasnim instrukcijama za UX agent
- **Rezultat:**
  - ✅ Definiran `.agent.md` sa:
    - Non-standard Bootstrap design direktivama
    - Gamification visual elements
    - Spiritual minimalist aesthetic
    - Color palette (Navy/Purple/Gold/Mint)
    - Responsive design requirements

### 3. **Sub-Agent Invocation - UI/UX Dorada**
- **Tema:** Trebam agenta da doradi CSS i view-ove
- **Odgovor:** Korištenje `runSubagent` tool-a za UX/UI Designer Agent
- **Rezultat:**
  - ✅ Agent je pozvan s detaljnim taskroom-om
  - ✅ Kreiran custom CSS framework (800+ linija)
  - ✅ Enhanced sve .cshtml view datoteke
  - ✅ Gamification visual elements implementirani:
    - XP progress bars s shimmer animacijom
    - Level badges s gradijentima
    - Activity-type specific color coding
    - Spiritual minimalist design
    - Full responsive design

### 4. **Mock Repository Implementacija**
- **Tema:** Trebam podatke iz Lab 1 dostupne kao repository
- **Odgovor:** Stvoriti UserMockRepository i ActivityMockRepository
- **Rezultat:**
  - ✅ UserMockRepository - svi 3 korisnika s aktivnostima i dnevnicima
  - ✅ ActivityMockRepository - pristup svim aktivnostima
  - ✅ Dependency injection registracija u Program.cs

### 5. **View Model Binding - RuntimeBinder Issue**
- **Tema:** Dashboard view ima error: 'does not contain Any()'
- **Odgovor:** Trebam strongly-typed DashboardViewModel umjesto dynamic objekta
- **Rezultat:**
  - ✅ Kreiran `DashboardViewModel.cs`
  - ✅ Ažuran `HomeController.Dashboard()` - vraća ViewModel
  - ✅ Ažurna `Dashboard.cshtml` s `@model` direktivom
  - ✅ Build succeeds, application radi bez greške

### 6. **Journal Display Issue - Text Overlap**
- **Tema:** Dnevnici se preklapaju - reflection tekst je sa raspoloženjem horizontalno
- **Odgovor:** Trebam CSS da osigura vertikalni raspored
- **Rezultat:**
  - ✅ Kreiran `journal-styles.css` s specifičnim stilovima
  - ✅ `.journal-item` koristi `flex-direction: column`
  - ✅ Svi elementi imaju `width: 100%`
  - ✅ Reflection je ispod raspoloženja, bez preklopa
  - ✅ Responsive mobile design

### 7. **Rasprava - Create/Edit Opcije za Lab 2**
- **Tema:** Trebam li implementirati Create/Edit ili to ostaviti za Lab 3?
- **Odgovor:** Lab 2 specifikacija je "bez Create/Edit opcija" - samo read-only
- **Rezultat:**
  - ✅ Aplikacija ostaje read-only
  - ✅ Index i Details stranice za sve entitete
  - ✅ Dashboard kao custom home page
  - ✅ Kompletna navigacija
  - ✅ Spremna za predaju po specifikaciji

---

## 🎯 Ključni Koncepti Koji Su Obrađeni

### ASP.NET MVC Arhitektura
- Model → View → Controller pattern
- Dependency Injection s `AddSingleton<>`
- Strongly-typed ViewModels

### HTML Binding u Razor
- Model binding - `@model` direktiva
- foreach loops s `@foreach`
- Conditionals - `@if`, `@else`
- Tag helpers - `asp-controller`, `asp-action`, `asp-route-*`
- HTML encoding - `@` prefix za C# kod

### Custom UI/UX Agent
- Definiranje `.agent.md` s instrukcijama
- Sub-agent invocation s detaljnim taskroom-om
- Praćenje AI-generated koda (CSS, HTML)
- Iterativna dorada nakon runtime greške

### CSS & Responsive Design
- Custom CSS framework bez Bootstrapa
- Gamification visual elements
- Mobile-first responsive design
- CSS variables za konsistentnost

---

## 📊 Ispit Rezultati

| Opis | Rezultat |
|------|----------|
| ASP.NET MVC Konverzija | ✅ Kompletna |
| Custom UI/UX Agent | ✅ Korišten, dokumentiran |
| Mock Repositories | ✅ UserMockRepository, ActivityMockRepository |
| Index/Details Stranice | ✅ Svi entiteti (User, Activity) |
| Custom Home Page | ✅ Dashboard sa statistikom |
| Kompletna Navigacija | ✅ Navbar, breadcrumbs, linkovi |
| Unique UX Design | ✅ Custom CSS, gamification, ne Bootstrap |
| Build Status | ✅ Bez greške |
| Application Running | ✅ http://localhost:5000 |
| Read-Only (po specifikaciji) | ✅ Nema Create/Edit |

---

## 🚀 Tehnologije & Alati

- **Framework**: ASP.NET MVC (.NET 10.0)
- **Language**: C# 12+
- **Frontend**: Razor + Custom CSS
- **Data**: Mock Repositories (iz Lab 1)
- **AI**: GitHub Copilot - Sub-agent za UX/UI
- **VCS**: Git + GitHub

---

## 📁 Datoteke Promijenjena/Kreirane

### Kreirane Datoteke
- `Program.cs` - MVC setup
- `Gamified Self Improvement.csproj` - Konvertiran u SDK.Web
- Controllers/:
  - `HomeController.cs`
  - `UserController.cs`
  - `ActivityController.cs`
- Models/:
  - `DashboardViewModel.cs`
- Repositories/:
  - `UserMockRepository.cs`
  - `ActivityMockRepository.cs`
- Views/:
  - Sve .cshtml datoteke
- CSS/:
  - `site.css` (custom framework)
  - `journal-styles.css` (journal display)

### Datoteke Premještene
- `Models/User.cs`, `Activities.cs`, `Enums.cs`, `GameDatabase.cs` (iz root)

### Datoteke Dokumentacije
- `.agent.md` - Custom UX/UI agent instrukcije
- `appsettings.json` - Konfiguracija
- `LAB-2-COMPLETION.md` - Projekta dokumentacija

---

## 🔍 Problemi Koji Su Riješeni

| Problem | Uzrok | Rješenje |
|---------|-------|---------|
| CS8802 - Top-level statements | 2 Program.cs datoteke | Premjestio Program-lab1.cs u lab-1/ |
| RuntimeBinder Exception | Dynamic object u view | Kreiran DashboardViewModel (strongly-typed) |
| Journal Text Overlap | Flexbox space-between | CSS flex-direction: column + width: 100% |
| Build Lock | dotnet.exe koristio bin/ | `dotnet clean` prije novog builda |

---

## 💡 Učeće Lekcije

1. **Model Binding je Kritično**: Nikad ne koristiti dynamic u view-om - uvijek strongly-typed model
2. **Sub-agent je Moćan**: Detaljnim instrukcijama (.agent.md) agent može generirati kompletan CSS framework
3. **CSS ima Prioritet**: `!important` někad je potreban da bi override-ao starije CSS
4. **Responsive Design**: Mobile-first pristup s `flex-wrap` i `width: 100%` sprječava preklope
5. **Mock Data je Fleksibilna**: Lako se može zamijeniti stvarnom bazom bez promjene controller-a

---

## 🎓 Zaključak

**Lab 2 je uspješno kompletiran** s:
- Novom ASP.NET MVC arhitekturom
- Custom-designed UI/UX (AI-enhanced)
- Kompletnom navigacijom
- Mock podatcima iz Lab 1
- Zero grešaka pri buildanju i radu

Aplikacija je spremna za predaju i demonstraciju koncepta MVC + HTML Binding + Model Binding + Custom UI/UX Agent.

---

**Završeno:** 9.4.2026  
**Status:** ✅ GOTOVO - Spremnu za Predaju
