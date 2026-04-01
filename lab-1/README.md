# Lab 1 - Gamified Self Improvement

## 📋 Pregled

Lab 1 predstavlja dizajn i implementaciju objektnog modela za **Gamified Self Improvement** aplikaciju. 
Projekt demonstrira C# koncepte: klase, svojstva, enume, 1-N relacije, LINQ upite, i async/await paradigmu.

---

## 📁 Struktura Foldara

```
lab-1/
├── AGENT-LOG.md          # Kompletan log AI agenta interakcija
├── chat-history.json     # Strukturirani razgovori u JSON formatu
└── README.md             # Ovaj fajl
```

---

## 🎯 Lab Zahtjevi

- ✅ **Objektni model**: 8 klasa (7+ zahtjevano)
- ✅ **Enumi**: 5 kreiranog (1+ zahtjevano)
- ✅ **Kompleksne klase**: 4 sa 5+ svojstava
- ✅ **DateTime svojstva**: 6+ (CreatedDate, CompletedDate, StartDate, JournalDate, LastActiveDate, UnlockedDate)
- ✅ **1-N relacije**: User → Activities, User → Journals
- ✅ **Test podaci**: 3 korisnika × 4 aktivnosti = 12 instanci
- ✅ **LINQ upiti**: 7 demonstrirano (OrderBy, Where, Cast, Take, GroupBy, Average, DateTime filter)
- ✅ **Async/Await**: SimulateMeditationAsync() sa Task.Delay()
- ✅ **GitHub**: Kod pushiran na: https://github.com/FranLiovicTVZ/Gamified-Self-improvement-Application
- ✅ **Kompilacija**: Bez grešaka (Exit Code: 0)

---

## 📚 Klase u Modelu

### 1. **User** (14+ svojstava)
```csharp
Id, Username, Email, CreatedDate, TotalXP, Level, 
Bio, ProfileImagePath, Activities, Journals, FavoriteBooks,
LastActiveDate, StreakDays, TotalExerciseMinutes
```
**Relacije**: 1-N sa Activity i DailyJournal

### 2. **Activity** (apstraktna, 6+ svojstava)
```csharp
Id, UserId, Title, Description, CompletedDate, 
XpReward, ActivityType, Difficulty
```
**Podtipovi**: Exercise, SpiritualActivity, Meditation, DailyJournal

### 3. **Exercise** (8 svojstava)
```csharp
DurationMinutes, CaloriesBurned, Sets, Reps, 
Weight, MuscleGroups, Location, ExerciseType
```

### 4. **SpiritualActivity** (8 svojstava)
```csharp
BookId, Book, PagesRead, CurrentPage, DurationMinutes,
Reflection, IsCompleted, StartDate
```

### 5. **Meditation** (7 svojstava)
```csharp
MeditationType, DurationMinutes, AudioFilePath, 
FocusArea, StressReliefScore, MentalClarity, Notes
```

### 6. **DailyJournal** (8 svojstava)
```csharp
JournalDate, DailyGoals, Ambitions, Accomplishments,
Reflection, Mood, EnergyLevel, Challenges
```

### 7. **SpiritualBook** (8 svojstava)
```csharp
Id, Title, BookType, TotalPages, Description, 
Author, Language, Chapters, IsAvailable
```

### 8. **GameDatabase** (4 svojstva)
```csharp
Users, SpiritualBooks, XpRewards, TrainingLogs
```

---

## 📊 Enumi

1. **ActivityType**: Exercise, Spiritual, Meditation, Journal
2. **ExerciseType**: Strength, Cardio, Flexibility, Sports
3. **SpiritualBookType**: Bible, Quran, Torah, Upanishads, BuddhaBhagavatam
4. **MeditationType**: Guided, Breathing, Mantras, Mindfulness, Visualization
5. **DifficultyLevel**: Easy, Medium, Hard, Extreme

---

## 🔍 LINQ Upiti Demonstrirani

```csharp
// 1. OrderByDescending - Korisnici sortirani po XP-u
var usersByXP = db.Users.OrderByDescending(u => u.TotalXP).ToList();

// 2. Where - Teške vježbe
var hardExercises = db.GetActivitiesByType(ActivityType.Exercise)
    .Cast<Exercise>()
    .Where(e => e.Difficulty == DifficultyLevel.Hard).ToList();

// 3. Take - Top 3 meditacije
var topMeditations = db.GetActivitiesByType(ActivityType.Meditation)
    .OrderByDescending(m => m.DurationMinutes).Take(3).ToList();

// 4. Any - Korisnici sa čitanjem 10+ stranica
var activeReaders = db.Users.Where(u => u.GetSpiritualActivities()
    .Any(s => s.PagesRead >= 10)).ToList();

// 5. GroupBy/Average - Prosječan XP po tipu aktivnosti
var xpByType = db.GetAllActivities()
    .GroupBy(a => a.ActivityType)
    .Select(g => new { Type = g.Key, AvgXP = g.Average(a => a.XpReward) })
    .ToList();

// 6. OrderBy - Korisnici sa streakovima
var userStreaks = db.Users.Where(u => u.StreakDays > 0)
    .OrderByDescending(u => u.StreakDays).ToList();

// 7. Where (DateTime) - Aktivnosti od danas
var todayActivities = db.GetAllActivities()
    .Where(a => a.CompletedDate.Date == DateTime.Now.Date).ToList();
```

---

## ⚡ Async/Await Demonstracija

```csharp
static async Task SimulateMeditationAsync()
{
    // Simulira 3 iteracije meditacije sa 5 faza po 800ms
    // Koristi await Task.Delay() - NE blokira thread
    // Omogućava paralelno čekanje bez blokiranja glavne niti
}
```

**Koncept**: 
- Async omogućava asinkronu izvršavanje
- Await pauzira metodu ALI ne blokira thread
- Razlika od Jave: Task je sličan CompletableFuture, ali elegantnije

---

## 📝 Test Korisnici

### 1. **Marko92** (XP: 3600, Level: 4)
- Vježbu: Bench Press (3 seta, 80kg)
- Duhovnu: Biblija (50 stranica)
- Meditacija: Breathing (8 minuta)
- Dnevnik: Refleksija dana

### 2. **AminaX** (XP: 3200, Level: 4)
- Vježbu: Jogging (45 minuta, 450 kalorija)
- Duhovnu: Kuran (60 stranica)
- Meditacija: Mindfulness (10 minuta)
- Dnevnik: Ambicije

### 3. **DavidT** (XP: 2800, Level: 3)
- Vježbu: Yoga (60 minuta, 300 kalorija)
- Duhovnu: Tora (40 stranica)
- Meditacija: Visualization (12 minuta)
- Dnevnik: Izazovi

---

## 🔧 TehnRugija

- **.NET**: 10.0
- **C#**: 12+
- **Paradigme**: OOP, LINQ, Async/Await
- **Verzija kontrola**: Git + GitHub

---

## 📖 Datoteke za Pregled

| Datoteka | Namjena |
|----------|---------|
| [AGENT-LOG.md](AGENT-LOG.md) | Kompletan log AI agenta razgovora s 13 dijelova |
| [chat-history.json](chat-history.json) | Strukturirani JSON format razgovora |
| `../Program.cs` | Glavna aplikacija sa test podacima i LINQ upitima |
| `../Enums.cs` | Definicije svih enuma |
| `../Activities.cs` | Sve klase aktività |
| `../User.cs` | User klasa sa agregacijom |
| `../GameDatabase.cs` | Database context i repository |

---

## 🎓 Ključni Koncepti Naučeni

1. **C# Properties**: Automatljena svojstva sa get/set (`{ get; set; }`)
2. **Nullable Reference Types**: `string?` znači null-able, `string` znači obavezno
3. **Auto-properties**: `public int Id { get; set; }` = automatskog backend fajla
4. **LINQ Chaining**: Fluentni API `query.Where().OrderBy().Select()...`
5. **Async/Await**: Non-blocking asinkrona izvršavanje
6. **Abstract Classes**: `Activity` kao bazna klasa za sve aktivnosti
7. **Enums**: Type-safe enumeracije umjesto stringa
8. **1-N Relacije**: Jedan korisnik → više aktivnosti

---

## ✅ Zaključak

Lab 1 uspješno demonstrira sve zahtjeve:
- Kompleksni objektni model sa 8 klasa
- Asinkrona programiranja sa async/await
- LINQ upiti za data manipulation
- Test podaci sa 3 korisnika i 12 aktivnosti
- GitHub integracija sa chat historijom

**Kod je uspješno kompajliran i spreman za produkciju.**

---

**Kreirano**: 1.4.2026.  
**Rok**: 2.4.2026. 18:00  
**Status**: ✅ Kompletan
