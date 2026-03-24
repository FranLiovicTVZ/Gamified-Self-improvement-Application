# Gamified Self Improvement - LAB 1

## 📋 Opis Projekta

Aplikacija za praćenje osobnog napretka kroz gamificirani sistem. Korisnici mogu bilježiti svoje aktivnosti (tjelovježba, duhovni razvitak, meditacija, dnevnik) i skupljati XP koji se pretvaraju u nivoe.

---

## ✅ LAB 1 Zahtjevi - ISPUNJENI

### 1. **Objektni Model - 8 Klasa (zahtjevano 7+)**

Kreirane klase:
1. ✅ **User** - Kompleksna (8 svojstava) - 1-N relacija s Activities
2. ✅ **Activity** - Bazna klasa (6 svojstava)
3. ✅ **Exercise** - Kompleksna (8 svojstava) - vježbanje u teretani
4. ✅ **SpiritualActivity** - Kompleksna (8 svojstava) - čitanje religijskih tekstova
5. ✅ **Meditation** - Kompleksna (7 svojstava) - meditacijske sesije
6. ✅ **DailyJournal** - Kompleksna (8 svojstava) - dnevni dnevnik
7. ✅ **SpiritualBook** - Kompleksna (5 svojstava) - Biblija, Kuran, Tora
8. ✅ **GameDatabase** - Globalna baza podataka

### 2. **Kompleksne Klase - 4 (zahtjevano 4)**

Klase sa 5+ svojstava:
- ✅ **User**: Id, Username, Email, CreatedDate, TotalXP, Level, Bio, ProfileImagePath, Activities, Journals, PreferredMeditationType, FavoriteBooks, StreakDays, LastActiveDate
- ✅ **Exercise**: DurationMinutes, CaloriesBurned, Sets, Reps, Weight, MuscleGroups, Location, ExerciseType
- ✅ **SpiritualActivity**: BookId, PagesRead, CurrentPage, DurationMinutes, Reflection, IsCompleted, StartDate
- ✅ **Meditation**: MeditationType, DurationMinutes, AudioFilePath, FocusArea, StressReliefScore, MentalClarity, Notes

### 3. **Enumi - Vlastiti (zahtjevano 1+)**

- ✅ **ActivityType** - Exercise, Spiritual, Meditation, Journal
- ✅ **ExerciseType** - Strength, Cardio, Flexibility, Sports
- ✅ **SpiritualBookType** - Bible, Quran, Torah, BuddhisticScriptures, Hindu
- ✅ **MeditationType** - Guided, Breathing, Mantras, Mindfulness, Visualization
- ✅ **DifficultyLevel** - Easy, Medium, Hard, Extreme

### 4. **DateTime Svojstva (zahtjevano 1+)**

- ✅ User.CreatedDate
- ✅ User.LastActiveDate
- ✅ Activity.CompletedDate
- ✅ DailyJournal.JournalDate
- ✅ SpiritualActivity.StartDate
- ✅ Meditation.CompletedDate

### 5. **Relacije 1-N i N-N (zahtjevano)**

- ✅ **1-N**: User (1) ---- (N) Activity (vježbe, meditacija, čitanja, dnevnik)
- ✅ **1-N**: User (1) ---- (N) DailyJournal
- ✅ **1-N**: SpiritualBook (1) ---- (N) SpiritualActivity (čitanja)

### 6. **3 Glavna Objekta sa 3+ Aktivnosti Svakog**

- ✅ **Marko92**: 4 aktivnosti (Bench Press, Čitanje Biblije, Meditacija, Dnevnik)
- ✅ **AminaX**: 4 aktivnosti (Jog, Čitanje Kurana, Meditacija, Dnevnik)
- ✅ **DavidT**: 4 aktivnosti (Yoga, Čitanje Tore, Meditacija, Dnevnik)

### 7. **LINQ Upiti (zahtjevano - razumjeti i moći modificirati)**

7 implementiranih LINQ upita:

1. **OrderByDescending** - Korisnici sortirani po XP
```csharp
var usersByXP = db.Users.OrderByDescending(u => u.TotalXP).ToList();
```

2. **Where + Cast** - Vježbe određene težine
```csharp
var hardExercises = db.GetActivitiesByType(ActivityType.Exercise)
    .Cast<Exercise>()
    .Where(e => e.Difficulty == DifficultyLevel.Hard)
    .ToList();
```

3. **OrderByDescending + Take** - Top 3 najdulje meditacije
```csharp
var longestMeditations = db.GetActivitiesByType(ActivityType.Meditation)
    .Cast<Meditation>()
    .OrderByDescending(m => m.DurationMinutes)
    .Take(3)
    .ToList();
```

4. **Where + Any + Select** - Aktivni čitatelji
```csharp
var activeReaders = db.Users
    .Where(u => u.GetSpiritualActivities().Any(s => s.PagesRead >= 10))
    .Select(u => new { u.Username, PagesRead = u.GetSpiritualActivities().Sum(s => s.PagesRead) })
    .ToList();
```

5. **GroupBy + Select** - Prosječan XP po vrsti aktivnosti
```csharp
var avgXpByType = db.GetAllActivities()
    .GroupBy(a => a.ActivityType)
    .Select(g => new { Type = g.Key, AvgXP = g.Average(a => a.CalculateXP()) })
    .ToList();
```

6. **Where + OrderByDescending** - Streakovi korisnika
```csharp
var userStreaks = db.Users
    .Where(u => u.StreakDays > 0)
    .OrderByDescending(u => u.StreakDays)
    .ToList();
```

7. **Where sa DateTime** - Aktivnosti od danas
```csharp
var todayActivities = db.GetAllActivities()
    .Where(a => a.CompletedDate.Date == DateTime.Now.Date)
    .ToList();
```

### 8. **Async-Await Koncept (zahtjevano)**

- ✅ Implementirana `SimulateMeditationAsync()` metoda
- ✅ Korištenje `await Task.Delay()` za simulaciju vremenskog kašnjenja
- ✅ Demonstracija kako async omogućava "ne-blokiranje" glavne dretve

```csharp
static async Task SimulateMeditationAsync()
{
    Console.WriteLine($"🧘 Započeta: {meditation.Title}\n");
    
    foreach (var phase in phases)
    {
        Console.WriteLine($"   {phase}");
        await Task.Delay(800); // Čeka bez blokiranje
    }
}
```

---

## 📁 Struktura Datoteka

```
Gamified Self Improvement/
├── Program.cs                    # Entry point + LINQ demo
├── ChatHistoryManager.cs         # Upravljanje chat istorijom
├── ChatMessage.cs                # Model za poruke
├── Enums.cs                      # Svi enumi aplikacije
├── Activities.cs                 # Svi tipovi aktivnosti
├── User.cs                       # User klasa + biznis logika
├── GameDatabase.cs               # Baza podataka + N-N relacije
├── Gamified Self Improvement.sln # Solution fajl
├── Gamified Self Improvement.csproj # Project fajl
└── Lab-1.md                      # (OVO FILE - LAB ZAHTJEVI)
```

---

## 🚀 Kako Pokrenuti

```bash
# Build
dotnet build

# Run
dotnet run
```

Aplikacija će:
1. Kreirati 3 korisnike s aktivnostima
2. Prikazati 7 LINQ upita
3. Simulirati async meditaciju
4. Omogućiti export chat istorije u JSON/TXT

---

## 💡 Ključne Karakteristike

### ✨ Objektni Model
- Nasljeđivanje: `Activity` je bazna klasa za Exercise, SpiritualActivity, Meditation
- Kolekcije: `List<T>` za sve relacije
- Enum-i za tipizaciju sustava

### 🎯 Poslovni Logika
- **XP Sustav**: Svaka aktivnost ima `CalculateXP()` metodu
- **Leveling**: User level se automatski ažurira
- **Streakovi**: Praćenje kontinuiteta aktivnosti
- **Analitika**: Average mood, favorite exercise type, total minutes

### 📊 LINQ Moć
- Filteriranje (`Where`)
- Sortiranje (`OrderBy`, `OrderByDescending`)
- Transformacija (`Select`)
- Grupiranje (`GroupBy`)
- Agregacija (`Sum`, `Average`, `Count`)
- Kombiniranje sa `DateTime` provjera

### ⚡ Async/Await
- `SimulateMeditationAsync()` demonstrira async pattern
- `await Task.Delay()` simulira vremenski proces
- Mogućnost dodavanja stvarnih async operacija (API pozivi, baza podataka)

---

## 📝 Dodatne Mogućnosti za Budućnost

- [ ] Implementacija `IDisposable` za pravljenje "audio meditacija" resursa
- [ ] Dodavanje `async` poziva za pravi API pristup Bibliji/Kuran/Thor
- [ ] Tracking napretka u striping (koliko stranica do sada)
- [ ] Notifikacije za postizanje milestona
- [ ] Export povijest u PDF formatom za prezentaciju
- [ ] Web API sa ASP.NET Core
- [ ] Database persistencija

---

## 📚 Koncepti Obrađeni

- ✅ Nasljeđivanje klasa
- ✅ Polimorfizam (abstract Activity klasa)
- ✅ Collections (List<T>, Dictionary)
- ✅ LINQ (WHERE, SELECT, GROUPBY, ORDERBY)
- ✅ DateTime rukovanje
- ✅ Async/Await pattern
- ✅ Event hooks (OnMessageAdded, OnHistoryExported)
- ✅ Enum-i i tipizacija
- ✅ Relacije između objekata (1-N, N-N)

---

## 🎓 Za Prezentaciju Projektu

Sve što je trebalo za Lab 1:
1. ✅ GitHub repozitorij (javiti link)
2. ✅ Objektni model s 8 klasa i 1-N relacijama
3. ✅ Podatci za 3 korisnika
4. ✅ 7 LINQ upita s razumijevanjem
5. ✅ Async-await demonstracija
6. ✅ Chat history za AI razgovore (za prezentaciju)

**Status**: Spreman za prezentaciju i predaju! 📦
