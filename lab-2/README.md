# Lab 2 - HTML/Binding - Gamified Self Improvement

## 📋 Pregled

Lab 2 implementira **ASP.NET MVC web aplikaciju** s custom UI/UX, MVC arhitekturom, mock repository-ima, i gamification elementima. Projekt demonstrira HTML binding, model binding, Razor view engine, dependency injection, i custom UI design.

---

## 📁 Struktura Projekta

```
Gamified Self Improvement/
├── Controllers/
│   ├── HomeController.cs
│   ├── UserController.cs
│   └── ActivityController.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ViewStart.cshtml
│   ├── Home/
│   │   └── Dashboard.cshtml
│   ├── User/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   └── Activity/
│       ├── Index.cshtml
│       └── Details.cshtml
├── Models/
│   ├── User.cs
│   ├── Activities.cs
│   ├── Enums.cs
│   ├── GameDatabase.cs
│   └── DashboardViewModel.cs
├── Repositories/
│   ├── UserMockRepository.cs
│   └── ActivityMockRepository.cs
├── wwwroot/css/
│   ├── site.css
│   └── journal-styles.css
├── .agent.md              # Custom UX/UI Agent Definition
└── Program.cs             # Dependency Injection Setup
```

---

## 🎯 Lab 2 Zahtjevi (Prema Specifikaciji)

| Kriterij | Status | Bodovi |
|----------|--------|--------|
| Prompt za sub-agenta za UI/UX | ✅ | 1 |
| Log da je sub-agent pozivan za UI/UX | ✅ | 1 |
| Napravljen unique UX koji radi s mock repository-ima | ✅ | 2 |
| Usmeno ispitivanje razumjevanja rada s custom agentima | ✅ | 1 |

### Dodatni Zahtjevi

- ✅ **Custom UX/UI Agent** (.agent.md) - Definiran s jasnim instrukcijama
- ✅ **Sub-agent Log** - Dokumentirano pozivanje UX agenta
- ✅ **Mock Repository** - UserMockRepository i ActivityMockRepository
- ✅ **Index Stranice** - Svi entiteti (User, Activity)
- ✅ **Details Stranice** - Profil korisnika, detalji aktivnosti
- ✅ **Custom Home Page** - Dashboard sa statistikom
- ✅ **Kompletna Navigacija** - Navbar, breadcrumbs, inter-linked entiteti
- ✅ **Unique UX** - Custom CSS (ne Bootstrap), gamification elementi
- ✅ **Model Binding** - Strongly-typed ViewModels (DashboardViewModel)
- ✅ **Dependency Injection** - Program.cs registra mock repository-je
- ✅ **Nije Create/Edit** - Samo read-only (po specifikaciji: "bez Create/Edit opcija")

---

## 🏗️ Arhitektura

### MVC Klasična Trijada

1. **Model** (Models/)
   - User, Activity (bazna), Exercise, SpiritualActivity, Meditation, DailyJournal
   - DashboardViewModel (strongly-typed za Dashboard view)

2. **View** (Views/)
   - Razor (.cshtml) s model binding
   - Custom .agent.md UI/UX direkcije
   - Gamification visual elements

3. **Controller** (Controllers/)
   - HomeController (Dashboard)
   - UserController (Index, Details)
   - ActivityController (Index, Details s filtering)

### Dependency Injection (Program.cs)

```csharp
builder.Services.AddSingleton<UserMockRepository>();
builder.Services.AddSingleton<ActivityMockRepository>();
builder.Services.AddSingleton<GameDatabase>();
```

Omogućava testiranje bez prave baze i laku zamjenu mock implementacija.

---

## 🎨 Custom UI/UX - Gamification Design

### Color Palette (Spiritual Minimalism)
- **Primary**: #1a1a3f (Deep Navy/Purple)
- **Secondary**: #663399 (Purple)
- **Accent**: #f4b860 (Warm Gold)
- **Success**: #4ecdc4 (Mint Green)

### Gamification Elements
- 📊 XP Progress Bars s shimmer animacijom
- 🏆 Level Badges s gradijent pozadinama
- ⚡ Activity Type Icons - Exercise 💪, Meditation 🧘, Spiritual 📖
- 📓 Journal Display s raspoloženjem i energijom (Mood/Energy scale 1-10)

### Not Bootstrap!
- Custom CSS u site.css i journal-styles.css
- Custom animations i transitions
- Spiritual minimalist estetika
- Responsive mobile-first design

---

## 📊 Podatkovni Modeli (iz Lab 1)

### 3 Glavna Korisnika
1. **Marko92** - Fitness entuzijast, 350 XP, Level 2
2. **AminaX** - Duhovni razvitak, 420 XP, Level 3
3. **DavidT** - Balans i mir, 280 XP, Level 2

### Aktivnosti po Korisniku
Svaki korisnik ima:
- 1 Exercise (vježba)
- 1 SpiritualActivity (čitanje)
- 1 Meditation
- 1 DailyJournal

**Ukupno**: 12 aktivnosti + 3 dnevnika dostupni iz mock repository-ja

---

## 🔧 Tehnički Detalji

### Razor View Engine - Binding Examples

**Strongly-typed Model**
```html
@model Gamified_Self_Improvement.Models.DashboardViewModel

<div class="users-grid">
    @foreach (var user in Model.Users.OrderByDescending(u => u.TotalXP).Take(6))
    {
        <div class="user-card">
            <h3>@user.Username</h3>
            <span class="level-badge">Level @user.Level</span>
            <p>@user.TotalXP XP</p>
        </div>
    }
</div>
```

**Model Binding iz Repositories**
```csharp
public IActionResult Dashboard()
{
    var dashboardViewModel = new DashboardViewModel
    {
        TotalUsers = users.Count,
        TopUser = users.OrderByDescending(u => u.TotalXP).FirstOrDefault(),
        RecentActivities = allActivities.OrderByDescending(a => a.CompletedDate).Take(5).ToList()
    };
    return View(dashboardViewModel);
}
```

### HTML View Structure

```html
<a asp-controller="User" asp-action="Details" asp-route-id="@user.Id">
    Vidi profil
</a>
```

Tag Helper za routing - bez hardkod URL-a.

---

## 🎯 Rute Aplikacije

| Ruta | Controller | Action | Opis |
|------|-----------|--------|------|
| `/` ili `/Home/Dashboard` | Home | Dashboard | Početna stranica sa statistikom |
| `/User/Index` | User | Index | Lista svih korisnika |
| `/User/Details/{id}` | User | Details | Profil korisnika + aktivnosti + dnevnici |
| `/Activity/Index` | Activity | Index | Lista svih aktivnosti |
| `/Activity/Details/{id}` | Activity | Details | Detalji aktivnosti po tipu |

---

## 🤖 Custom UI/UX Agent (.agent.md)

### Agent Definition
- Specijalizirani agent za UI/UX design decisions
- Jasne direktive: non-standard design, gamification, responsive
- Može mijenjati: .cshtml, site.css, design assets
- Ne može: SQL, C# logika, dependency management

### Sub-Agent Invocation
Agent je pozvan s taskroom:
```
"Enhance UI/UX with custom gamification design"
- Doradi CSS s XP bars, level badges
- Kreiraj unique spiritual minimalist design
- Implementira responsive mobile-first
- Dodaj animation i transitions
```

**Rezultat**: Kompletno custom CSS framework + enhanced view strukture

---

## ✅ Testing & Deployment

### Build & Run
```bash
dotnet build          # ✅ Bez greške
dotnet run           # ✅ Sluša na http://localhost:5000
```

### Testirane Rute
- ✅ Dashboard - prikazuje korisnika i aktivnosti
- ✅ User/Index - lista svih korisnika
- ✅ User/Details/1-3 - profili s aktivnostima i dnevnicima
- ✅ Activity/Index - sve aktivnosti
- ✅ Activity/Details - detalji po tipu aktivnosti

---

## 📝 Zaključak Lab 2

**Kompletna ASP.NET MVC read-only aplikacija** s:
- Custom UI/UX agent-enhanced dizajnom
- Mock data iz Lab 1
- Proper MVC arhitekturom
- Dependency injection
- Gamification visual design
- Responsive CSS

**Spremna za predaju**: Sav kod na Git, dokumentacija gotova, všće zahtjeve ispunjeni.

---

**Projekt**: Gamified Self Improvement - Lab 2 HTML/Binding  
**Datum**: 9.4.2026  
**Status**: ✅ GOTOVO
