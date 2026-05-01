# Semantic DB Model - Gamified Self Improvement

## Opis baze podataka

Baza podataka `gamified_self_improvement` sadrži sve potrebne tabele i relacije za aplikaciju koja omogućava korisnike da prate svoje napredovanje u vježbanju, meditaciji, čitanju duhovnih tekstova i vođenju dnevnika.

## Tabele/Entiteti

### 1. **Users** (Korisnici)
Sadržava sve korisnike aplikacije.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `Id` | int (PK) | Jedinstveni identifikator korisnika |
| `Username` | string | Korisničko ime |
| `Email` | string | Email adresa korisnika |
| `CreatedDate` | datetime | Datum kreiranja korisnika |
| `TotalXP` | int | Ukupno XP prikupljeno |
| `Level` | int | Trenutni level |
| `Bio` | string | Biografija korisnika |
| `ProfileImagePath` | string | Put do profilne slike |
| `PreferredMeditationType` | enum | Preferirana vrsta meditacije |
| `StreakDays` | int | Broj dana aktivnosti u nizu |
| `LastActiveDate` | datetime | Zadnja aktivnost korisnika |

**1-N Relacije:**
- `Users` → `Activities` (jedan korisnik ima više aktivnosti)
- `Users` → `DailyJournals` (jedan korisnik ima više dnevnika)

---

### 2. **Activities** (Aktivnosti - Bazna klasa)
Bazna tablica za sve vrste aktivnosti (nasljeđivanje).

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `Id` | int (PK) | Jedinstveni identifikator |
| `UserId` | int (FK) | Pripadajući korisnik |
| `Title` | string | Naziv aktivnosti |
| `Description` | string | Opis aktivnosti |
| `CompletedDate` | datetime | Datum završetka |
| `XpReward` | int | XP nagrađen |
| `ActivityType` | enum (Discriminator) | Tip aktivnosti (Exercise, Spiritual, Meditation, Journal) |
| `Difficulty` | enum | Težina aktivnosti |

**Nasljeđene klase:**
- `Exercises` - vježbe
- `SpiritualActivities` - duhovne aktivnosti
- `Meditations` - meditacije
- `DailyJournals` - dnevnici

---

### 3. **Exercises** (Vježbe)
Fizičke vježbe i treninge.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `ExerciseType` | enum | Tip vježbe (Strength, Cardio, Flexibility, Sports) |
| `DurationMinutes` | int | Trajanje u minutama |
| `CaloriesBurned` | int | Spaljene kalorije |
| `Sets` | int | Broj setova |
| `Reps` | int | Broj ponavljanja |
| `Weight` | decimal | Težina (kg) |
| `MuscleGroups` | string array | Grupe mišića zadete vježbom |
| `Location` | string | Lokacija vježbanja |

---

### 4. **SpiritualActivities** (Duhovne aktivnosti)
Čitanje duhovnih tekstova.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `BookId` | int (FK) | Riferencirani duhovni tekst |
| `PagesRead` | int | Broj pročitanih stranica |
| `CurrentPage` | int | Trenutna stranica |
| `DurationMinutes` | int | Trajanje čitanja |
| `Reflection` | string | Refleksija na tekst |
| `IsCompleted` | bool | Je li knjiga dovršena |
| `StartDate` | datetime | Datum početka čitanja |

**Relacije:**
- `SpiritualActivities` → `SpiritualBooks` (FK)

---

### 5. **Meditations** (Meditacije)
Meditacijske sesije.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `MeditationType` | enum | Tip meditacije (Guided, Breathing, Mantras, Mindfulness, Visualization) |
| `DurationMinutes` | int | Trajanje sesije |
| `AudioFilePath` | string | Put do audio datoteke |
| `FocusArea` | string | Područje fokusa |
| `StressReliefScore` | int (1-10) | Ocjena razvijenosti stresa |
| `MentalClarity` | int (1-10) | Ocjena mentalne jasnoće |
| `Notes` | string | Napomene |

---

### 6. **DailyJournals** (Dnevnici)
Dnevne refleksije i ciljevi.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `JournalDate` | datetime | Datum dnevnika |
| `DailyGoals` | string array | Dnevni ciljevi |
| `Ambitions` | string array | Ambicije |
| `Accomplishments` | string array | Ostvarena dostignuća |
| `Reflection` | string | Refleksija dana |
| `Mood` | int (1-10) | Raspoloženje |
| `EnergyLevel` | int (1-10) | Energija |
| `Challenges` | string array | Izazovi i prepreke |

---

### 7. **SpiritualBooks** (Duhovni tekstovi)
Dostupni duhovni tekstovi.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `Id` | int (PK) | Jedinstveni identifikator |
| `Title` | string | Naziv knjige |
| `BookType` | enum | Tip knjige (Bible, Quran, Torah, etc.) |
| `TotalPages` | int | Ukupan broj stranica |
| `Description` | string | Opis |
| `Author` | string | Autor |
| `Language` | string | Jezik |
| `Chapters` | string array | Poglavlja |
| `IsAvailable` | bool | Je li dostupna |

---

### 8. **XPRewards** (XP Nagrade)
Dostupne XP nagrade.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `Id` | int (PK) | Jedinstveni identifikator |
| `Name` | string | Naziv nagrade |
| `Description` | string | Opis nagrade |
| `XpAmount` | int | Broj XP bodova |
| `ActivityType` | enum | Tip aktivnosti |
| `UnlockedDate` | datetime | Datum otključavanja |
| `Icon` | string | Emoji ikona |

---

### 9. **TrainingLogs** (Logovi treninga)
Detaljni logovi treninga za praksu.

| Svojstvo | Tip | Opis |
|----------|-----|------|
| `Id` | int (PK) | Jedinstveni identifikator |
| `ExerciseId` | int (FK) | Vježba |
| `UserId` | int (FK) | Korisnik |
| `ExerciseName` | string | Naziv vježbe |
| `Weight` | decimal | Težina |
| `Sets` | int | Setovi |
| `Reps` | int | Ponavljanja |
| `RestSeconds` | int | Pauza između setova |
| `LogDate` | datetime | Datum loga |
| `Notes` | string | Napomene |
| `Difficulty` | enum | Težina |

---

## Veze između entiteta

```
Users
├─ 1 ─ N ─ Activities
│           ├─ Exercises
│           ├─ SpiritualActivities ─(FK)─ SpiritualBooks
│           ├─ Meditations
│           └─ DailyJournals
├─ 1 ─ N ─ DailyJournals
└─ Implicit: Users.FavoriteBooks → SpiritualBooks ids

XPRewards (lookup tablica)
SpiritualBooks (lookup tablica)
TrainingLogs (istorija treninga)
```

## Principi nasljeđivanja

Koristi se **Table Per Hierarchy (TPH)** pristup gdje je `Activity` bazna klasa sa `Discriminator` stupcem koji razlikuje tip aktivnosti:
- `Exercise`
- `Spiritual`
- `Meditation`
- `Journal`

Sve aktivnosti dijele osnovne svojstva (Id, UserId, Title, Description, CompletedDate, XpReward, ActivityType, Difficulty) i imaju dodatna specifična svojstva prema vrsti.
