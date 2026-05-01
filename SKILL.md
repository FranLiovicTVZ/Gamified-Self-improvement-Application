---
name: Entity Framework Skill
description: Skill za upravljanje Entity Framework entitetima, kreiranjem migracija i rad s bazom podataka
patterns:
  - "add.*property|field|column.*model"
  - "create.*migration|update.*database"
  - "modify.*entity|table|model"
  - "add.*relationship|foreign key"
  - "configure.*ef|entity framework"
---

# Entity Framework Skill

Ovaj skill se koristi kada trebate dodati izmjene u EF entitete, generirati nove migracije ili ažurirati bazu podataka.

## Kontekst

Projekt koristi:
- **Framework**: ASP.NET Core 10
- **ORM**: Entity Framework Core 10.0.7
- **Baza**: SQL Server (Docker)
- **DbContext**: `GamefiedSelfImprovementDbContext` (Models/)
- **Repositories**: `UserRepository.cs` i `ActivityRepository.cs` (Repositories/)

## Standardni workflow

### 1. Modificiranje modela

Prije bilo kakvih promjena u bazi, trebate:

1. Dodati/modificirati svojstva u C# klasu (Models/)
2. Ako je potrebno, dodati `[Key]`, `[ForeignKey]`, ili `[Required]` anotacije
3. Ako se dodaju 1-N relacije, koristiti `virtual ICollection<T>`

**Primjer:**
```csharp
public class Activity
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    public virtual User? User { get; set; }
}
```

### 2. Kreiranje migracije

Nakon promjena u modelu, trebate generirat migraciju:

```bash
cd "c:\Users\fran\OneDrive\Radna površina\faks\.NET\Gamified Self Improvement"
dotnet-ef migrations add [NazivMigracije] --context GamefiedSelfImprovementDbContext
```

**Primjeri:**
- `dotnet-ef migrations add AddExerciseTable --context GamefiedSelfImprovementDbContext`
- `dotnet-ef migrations add ModifyActivityModel --context GamefiedSelfImprovementDbContext`

### 3. Primjena migracije na bazu

```bash
dotnet-ef database update --context GamefiedSelfImprovementDbContext
```

### 4. Ako trebate poništiti migraciju

```bash
dotnet-ef migrations remove --context GamefiedSelfImprovementDbContext -f
```

## Česti scenariji

### Dodavanje novog polja u postojeću tablicu

1. Dodajte svojstvo u C# klasu:
```csharp
public string NewField { get; set; }
```

2. Kreirajte migraciju:
```bash
dotnet-ef migrations add AddNewFieldToActivity --context GamefiedSelfImprovementDbContext
```

3. Primijenite:
```bash
dotnet-ef database update --context GamefiedSelfImprovementDbContext
```

### Dodavanje nove tablice/entiteta

1. Kreirajte novu klasu u Models/ sa `[Key]` anotacijom na Id
2. Dodajte `DbSet<NovaKlasa>` u GamefiedSelfImprovementDbContext
3. Dodajte konfiguraciju u `OnModelCreating()` ako je potrebna
4. Kreirajte migraciju
5. Primijenite migraciju

### Promjena relacije između tablica

1. Dodajte `[ForeignKey]` anotaciju na svojstvo Id
2. Dodajte `virtual ICollection<T>` na baznu klasu
3. Kreirajte migraciju
4. Primijenite migraciju

## Važne napomene

⚠️ **Seed podatci** trebaju biti **statički** (fiksne vrijednosti), ne dinamički:
```csharp
// ✓ DOBRO
new { Id = 1, Name = "Test", CreatedDate = new DateTime(2026, 4, 30) }

// ✗ LOŠE
new { Id = 1, Name = "Test", CreatedDate = DateTime.Now }
```

⚠️ **Decimal svojstva** trebaju biti eksplicitno konfigurirana:
```csharp
modelBuilder.Entity<Exercise>()
    .Property(e => e.Weight)
    .HasPrecision(5, 2); // 5 znamenki, 2 decimale
```

## Repositories

Projekt koristi repository pattern:

- **UserRepository**: CRUD operacije za korisnike
- **ActivityRepository**: CRUD operacije za aktivnosti

Koriste se kroz dependency injection u controllerima.

## Struktura modela

```
User (1-N) Activity
  ├─ Exercise
  ├─ SpiritualActivity
  ├─ Meditation
  └─ DailyJournal

SpiritualBook (lookup tablica)
XPReward (lookup tablica)
TrainingLog (istorija)
```

## Redoslijed zadataka kod promjena

1. ✅ Modificiraj model (C# klasu)
2. ✅ Kreiraj migraciju (`migrations add`)
3. ✅ Pregledaj generirani kod u Migrations/[timestamp]_[NazivMigracije].cs
4. ✅ Provjeri build (`dotnet build`)
5. ✅ Primijeni migraciju (`database update`)
6. ✅ Ažuriraj repository ako je potrebno
7. ✅ Ažuriraj controllere/view-ove ako trebaju nove kolone

## Kompleksne operacije

Ako trebate kompleksniju migraciju (npr. prenamena stupca, promjena tipa podataka), možete:

1. Ručno urediti generirani kod u Migrations/
2. Koristiti raw SQL u migraciji:
```csharp
migrationBuilder.Sql("UPDATE Activities SET NewColumn = OldColumn");
```

## Troubleshooting

### Greška: "Could not load assembly"
- Trebali bi rebuild: `dotnet build`

### Greška: "Model validation failed"
- Provjerite seed podatke - trebaju biti statički
- Provjerite decimal svojstva - trebaju imati `HasPrecision()`

### Greška: "Foreign key property created in shadow state"
- Eksplicitno definirajte FK svojstvo sa anotacijom `[ForeignKey]`

## Korisni linkovi

- [EF Core Docs](https://docs.microsoft.com/en-us/ef/core)
- [Migrations Reference](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations)
- [Data Seeding](https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding)
