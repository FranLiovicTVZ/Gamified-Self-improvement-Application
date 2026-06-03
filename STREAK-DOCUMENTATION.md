# Streak Entitet - Dokumentacija

## 📋 Sažetak

Dodan **Streak entitet** koji automatski broji koliko je dana za redom korisnik obavljao aktivnosti. Entitet uključuje:
- Trenutni streak (broj dana za redom)
- Najduži streak (best record)
- Datum zadnje aktivnosti
- Ukupan broj obavljenih aktivnosti
- Automatsku logiku za resetiranje streaka ako korisnik bude neaktivan

---

## 🆕 Što je Dodano

### 1. Model - Streak.cs

**Lokacija**: `Models/Streak.cs`

```csharp
public class Streak
{
    public int Id { get; set; }
    public string UserId { get; set; }                    // FK na AppUser
    public virtual AppUser? AppUser { get; set; }
    
    public int CurrentStreak { get; set; }                // Broj dana za redom
    public int LongestStreak { get; set; }                // Best record
    public DateTime? LastActivityDate { get; set; }       // Zadnja aktivnost
    public DateTime? LastStreakResetDate { get; set; }    // Zadnji reset
    public int TotalActivitiesCompleted { get; set; }     // Ukupno aktivnosti
    
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    // Metode:
    public void RecordActivity()                          // Zapiši aktivnost
    public bool CheckAndResetStreak()                     // Provjeri reset
}
```

**Svojstva**:
- `Id` - Primary key
- `UserId` - Foreign key na AppUser (one-to-one veza)
- `CurrentStreak` - Broj dana aktivnosti za redom (resetira se ako ima pauze)
- `LongestStreak` - Best record svih vremena
- `LastActivityDate` - Datum zadnje obavljene aktivnosti
- `LastStreakResetDate` - Kada je zadnji put resetiran streak
- `TotalActivitiesCompleted` - Broj ukupnih aktivnosti
- `CreatedDate` - Kada je napravljen Streak record
- `UpdatedDate` - Kada je zadnja izmjena

**Metode**:
- `RecordActivity()` - Automatski logika:
  - Ako je aktivnost obavljena ista dan → nema promjene
  - Ako je aktivnost obavljena jučer → nastavi streak
  - Ako su preskočeni dani → resetiraj streak na 1
  - Auto-update `LongestStreak` ako je trebao

- `CheckAndResetStreak()` - Provjerava ako više od 1 dana nije bilo aktivnosti:
  - Ako da → resetira `CurrentStreak` na 0
  - Ako ne → ne mijenja ništa

### 2. DbContext Update

**Datoteka**: `Models/GamefiedSelfImprovementDbContext.cs`

Dodano u DbContext:
```csharp
public DbSet<Streak> Streaks { get; set; }  // Nova tablica
```

### 3. AppUser Update

**Datoteka**: `Models/AppUser.cs`

Dodana navigacijska svojstva:
```csharp
public virtual Streak? Streak { get; set; }  // One-to-one sa Streak
```

### 4. DTO Klase

**Datoteka**: `DTOs/OtherDTOs.cs`

```csharp
public class StreakDTO
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? LastStreakResetDate { get; set; }
    public int TotalActivitiesCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class UpdateStreakDTO
{
    public string UserId { get; set; }  // Za record-activity endpoint
}
```

### 5. API Controller - StreaksApiController.cs

**Lokacija**: `Controllers/Api/StreaksApiController.cs`

**Route**: `/api/streaks`

#### Endpointi:

1️⃣ **GET** `/api/streaks` - Sve streake
   - **Autorizacija**: `[Authorize(Roles = "Admin,Manager")]`
   - **Vraća**: List<StreakDTO>
   - **Opis**: Dohvat svih streaka (samo za admin/manager)

2️⃣ **GET** `/api/streaks/{id}` - Jedan streak
   - **Autorizacija**: `[Authorize]`
   - **Vraća**: StreakDTO
   - **Opis**: Korisnik može vidjeti samo svoj streak (ili admin)

3️⃣ **GET** `/api/streaks/user/{userId}` - Streak za specifičnog korisnika
   - **Autorizacija**: `[Authorize]`
   - **Vraća**: StreakDTO
   - **Logika**: Ako Streak ne postoji → kreira novi
   - **Opis**: Dohvat streaka za korisnika

4️⃣ **POST** `/api/streaks/record-activity` - Zapiši obavljenu aktivnost
   - **Autorizacija**: `[Authorize]`
   - **Body**: 
     ```json
     {
       "userId": "user-id-string"
     }
     ```
   - **Vraća**: StreakDTO (ažurirani)
   - **Logika**:
     - Provjeri ako trebam resetirati streak (>1 dan bez aktivnosti)
     - Zapiši novu aktivnost
     - Ažuriraj CurrentStreak i LongestStreak
   - **Opis**: Glavna metoda - poziva se nakon što korisnik obavi aktivnost

5️⃣ **PUT** `/api/streaks/{id}` - Ažuriraj streak
   - **Autorizacija**: `[Authorize(Roles = "Admin")]`
   - **Body**: StreakDTO (sa novim vrijednostima)
   - **Vraća**: StreakDTO
   - **Opis**: Samo admin može ažurirati

6️⃣ **DELETE** `/api/streaks/{id}` - Obriši streak
   - **Autorizacija**: `[Authorize(Roles = "Admin")]`
   - **Vraća**: `{ message: "Streak je uspješno obrisan" }`
   - **Opis**: Samo admin može obrisati

7️⃣ **GET** `/api/streaks/top/leaderboard` - Top 10 streakova
   - **Autorizacija**: `[AllowAnonymous]` (javno)
   - **Vraća**: Ranked lista top 10 korisnika
   - **Sortiranje**: Po `LongestStreak` (najduži), zatim `CurrentStreak`
   - **Response**:
     ```json
     [
       {
         "rank": 1,
         "userId": "user-id",
         "userName": "ivan_temuhin",
         "currentStreak": 5,
         "longestStreak": 25,
         "totalActivities": 120
       },
       ...
     ]
     ```
   - **Opis**: Javna leaderboard sa top 10 aktivnih korisnika

### 6. BaseApiController Update

**Datoteka**: `Controllers/Api/BaseApiController.cs`

Dodana mapping metoda:
```csharp
protected StreakDTO MapStreakToDTO(Streak streak)
{
    return new StreakDTO { ... };
}
```

---

## 🔄 Logika rada Streaka

### Scenarij 1: Korisnik obavlja aktivnost prvi put
```
RecordActivity() pozivanog:
- CurrentStreak = 1
- LastActivityDate = Danas
- LongestStreak = 1
```

### Scenarij 2: Korisnik obavlja aktivnost sutranji dan
```
CheckAndResetStreak() vraća false (nije trebalo resetirati)
RecordActivity() pozvanog:
- CurrentStreak = 2  (nastavljen)
- LastActivityDate = Sutranji dan
- LongestStreak = 2 (ažuriran jer je > 1)
```

### Scenarij 3: Korisnik preskočio 2 dana
```
CheckAndResetStreak() vraća true (DaysSinceLast = 3)
RecordActivity() pozvanog:
- CurrentStreak = 1  (resetiran jer je >1 dan prošlo)
- LastActivityDate = Danas
- LastStreakResetDate = Danas
```

### Scenarij 4: Korisnik obavlja 2 aktivnosti isti dan
```
RecordActivity() prvi put:
- CurrentStreak = 1
- LastActivityDate = Danas

RecordActivity() drugi put isti dan:
- CurrentStreak = 1  (bez promjene - već je obavio aktivnost danas)
```

---

## 📊 API Primjeri

### Zapiši aktivnost (Najčešće korišteno)
```http
POST /api/streaks/record-activity
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Odgovor** (200 OK):
```json
{
  "id": 1,
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "currentStreak": 5,
  "longestStreak": 12,
  "lastActivityDate": "2026-06-01T10:06:38.000Z",
  "lastStreakResetDate": "2026-05-28T10:06:38.000Z",
  "totalActivitiesCompleted": 45,
  "createdDate": "2026-06-01T08:00:00.000Z",
  "updatedDate": "2026-06-01T10:06:38.000Z"
}
```

### Dohvati leaderboard
```http
GET /api/streaks/top/leaderboard
```

**Odgovor** (200 OK):
```json
[
  {
    "rank": 1,
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "userName": "ivan_temuhin",
    "currentStreak": 15,
    "longestStreak": 35,
    "totalActivities": 250
  },
  {
    "rank": 2,
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "userName": "marija_fitness",
    "currentStreak": 10,
    "longestStreak": 28,
    "totalActivities": 180
  }
]
```

### Dohvati svoj streak
```http
GET /api/streaks/user/{userId}
Authorization: Bearer {token}
```

---

## 🔌 Integracija sa ExercisesApiController (Primjer)

Kada korisnik kreira novu vježbu, trebao bi pozvati:

```csharp
// U ExercisesApiController.Create()
if (exercise.Id > 0)  // Nakon što je vježba kreirana
{
    // Zapiši u Streak
    var client = new HttpClient();
    await client.PostAsJsonAsync(
        "http://localhost:5000/api/streaks/record-activity",
        new { userId = currentUserId }
    );
}
```

Ili lakše - direktno iz baze:
```csharp
var streak = await _dbContext.Streaks
    .FirstOrDefaultAsync(s => s.UserId == userId);

if (streak == null)
    streak = new Streak(userId);

streak.RecordActivity();
_dbContext.Streaks.Update(streak);
await _dbContext.SaveChangesAsync();
```

---

## ✅ Testiranje API Endpointa

Svi endpointi su testirani i vraćaju ispravan odgovore:

✅ **GET** `/api/streaks/top/leaderboard` - Vraća praznu listu `[]`
```
Status: 200 OK
Response: []
```

---

## 📁 Sažetak Datoteka

| Datoteka | Akcija | Opis |
|----------|--------|------|
| `Models/Streak.cs` | ✨ NOVO | Novi entitet sa logikom |
| `Models/AppUser.cs` | AŽURIRAN | Dodana `Streak?` svojstva |
| `Models/GamefiedSelfImprovementDbContext.cs` | AŽURIRAN | `DbSet<Streak>` |
| `DTOs/OtherDTOs.cs` | AŽURIRAN | `StreakDTO`, `UpdateStreakDTO` |
| `Controllers/Api/StreaksApiController.cs` | ✨ NOVO | 7 endpointa |
| `Controllers/Api/BaseApiController.cs` | AŽURIRAN | `MapStreakToDTO()` metoda |

---

## 🎯 Korištenje u Aplikaciji

### Za frontend developer:
1. **Nakon što korisnik obavi aktivnost** → Pozovi `POST /api/streaks/record-activity`
2. **Prikaži trenutni streak** → Dohvati `GET /api/streaks/user/{userId}`
3. **Prikaži leaderboard** → Koristi `GET /api/streaks/top/leaderboard`

### Za admin:
1. **Vidi sve streake** → `GET /api/streaks` (samo Manager/Admin)
2. **Ažuriraj streak** → `PUT /api/streaks/{id}` (samo Admin)
3. **Obriši streak** → `DELETE /api/streaks/{id}` (samo Admin)

---

## 📊 Baza Podataka - Streak Tablica

```sql
Streaks
├── Id (int, PK)
├── UserId (string, FK)
├── CurrentStreak (int)
├── LongestStreak (int)
├── LastActivityDate (datetime nullable)
├── LastStreakResetDate (datetime nullable)
├── TotalActivitiesCompleted (int)
├── CreatedDate (datetime)
└── UpdatedDate (datetime)

Indeks: UserId (unique) - One-to-one sa AppUser
```

---

## ✨ Primjer Gameifikacije

Osnovna gameifikacijska struktura sada:
1. ✅ **XP sustav** - Korisnik dobije XP po aktivnosti
2. ✅ **Level sustav** - Penjanje levela sa XP
3. ✅ **Streak sustav** (NOVO) - Motivacija za svakodnevnu aktivnost
4. ✅ **Leaderboard** - Kompeticija sa drugim korisnicima
5. ✅ **Achievements** (može se dodati) - Badges za milestones

Streak je ključan za motivaciju korisnika da se **vraća svaki dan**! 🔥

