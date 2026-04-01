using GamefiedSelfImprovement;
using System.Linq;

// ========== KREIRAJ BAZU PODATAKA ==========
Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║   GAMIFIED SELF IMPROVEMENT - LAB 1 DEMO          ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

var db = new GameDatabase();

// ========== KREIRAJ 3 GLAVNA KORISNIKA ==========
Console.WriteLine("📍 Kreiranje 3 glavna korisnika...\n");

// KORISNIK 1 - Marko
var marko = new User(1, "Marko92", "marko@example.com")
{
    Bio = "Fitness entuzijast, čita Bibliju",
    PreferredMeditationType = MeditationType.Breathing
};

// Marko - Aktivnost 1: Teretana
var markoVjezba1 = new Exercise(marko.Id, "Bench Press sesija", ExerciseType.Strength)
{
    DurationMinutes = 45,
    Sets = 4,
    Reps = 10,
    Weight = 80,
    Difficulty = DifficultyLevel.Hard,
    CaloriesBurned = 350,
    Location = "Teretana",
    MuscleGroups = new() { "Prsni mišići", "Tricepsi" }
};

// Marko - Aktivnost 2: Čitanje
var markoChitanje = new SpiritualActivity(marko.Id, 1, "Čitanje Biblije")
{
    BookId = 1,
    Book = db.SpiritualBooks[0],
    CurrentPage = 45,
    PagesRead = 15,
    DurationMinutes = 30,
    Reflection = "Duboka poruka o vjeri i naději.",
    Difficulty = DifficultyLevel.Medium
};

// Marko - Aktivnost 3: Meditacija
var markoMeditacija = new Meditation(marko.Id, "Jutarnja meditacija", MeditationType.Breathing, 20)
{
    FocusArea = "Smirenje uma",
    StressReliefScore = 8,
    MentalClarity = 7,
    Difficulty = DifficultyLevel.Easy,
    Notes = "Osjećam se mirnije i fokusiranije"
};

// Marko - Aktivnost 4: Dnevnik
var markoDnevnik = new DailyJournal(marko.Id, "Dnevnik 24.03.2026")
{
    Mood = 8,
    EnergyLevel = 7
};
markoDnevnik.AddGoal("Završiti teretanu");
markoDnevnik.AddGoal("Pročitati 20 stranica Biblije");
markoDnevnik.AddAmbition("Postati jači");
markoDnevnik.Reflection = "Odličan dan, motiviran za nastavak.";

marko.AddActivity(markoVjezba1);
marko.AddActivity(markoChitanje);
marko.AddActivity(markoMeditacija);
marko.AddJournal(markoDnevnik);

db.AddUser(marko);

// KORISNIK 2 - Amira
var amira = new User(2, "AminaX", "amina@example.com")
{
    Bio = "Islamska vjera, duhovni razvitak",
    PreferredMeditationType = MeditationType.Mantras
};

// Amira - Aktivnost 1: Jog
var amiraJog = new Exercise(amira.Id, "Jog u parku", ExerciseType.Cardio)
{
    DurationMinutes = 40,
    Sets = 1,
    Reps = 1,
    Weight = 0,
    Difficulty = DifficultyLevel.Medium,
    CaloriesBurned = 400,
    Location = "Park",
    MuscleGroups = new() { "Noge", "Kardiovaskularni sustav" }
};

// Amira - Aktivnost 2: Čitanje Kurana
var amiraKuran = new SpiritualActivity(amira.Id, 2, "Čitanje Kurana")
{
    BookId = 2,
    Book = db.SpiritualBooks[1],
    CurrentPage = 120,
    PagesRead = 20,
    DurationMinutes = 45,
    Reflection = "Muhammadove poruke su vječne i relevantne.",
    Difficulty = DifficultyLevel.Hard
};

// Amira - Aktivnost 3: Meditacija
var amiraMeditacija = new Meditation(amira.Id, "Noćna meditacija", MeditationType.Mindfulness, 25)
{
    FocusArea = "Razumijevanje sebe",
    StressReliefScore = 9,
    MentalClarity = 8,
    Difficulty = DifficultyLevel.Medium
};

// Amira - Aktivnost 4: Dnevnik
var amiraDnevnik = new DailyJournal(amira.Id, "Dnevnik 24.03.2026")
{
    Mood = 9,
    EnergyLevel = 8
};
amiraDnevnik.AddGoal("Završiti jog");
amiraDnevnik.AddGoal("Pročitati 20 stranica Kurana");
amiraDnevnik.AddAmbition("Biti bolji čovjek");
amiraDnevnik.Reflection = "Osjećam veću duhovnu povezanost.";

amira.AddActivity(amiraJog);
amira.AddActivity(amiraKuran);
amira.AddActivity(amiraMeditacija);
amira.AddJournal(amiraDnevnik);

db.AddUser(amira);

// KORISNIK 3 - David
var david = new User(3, "DavidT", "david@example.com")
{
    Bio = "Korisnik Jude, brojanje kalorijaa",
    PreferredMeditationType = MeditationType.Visualization
};

// David - Aktivnost 1: Vježbanje fleksibilnosti
var davidFleksibilnost = new Exercise(david.Id, "Yoga sesija", ExerciseType.Flexibility)
{
    DurationMinutes = 50,
    Sets = 0,
    Reps = 0,
    Weight = 0,
    Difficulty = DifficultyLevel.Medium,
    CaloriesBurned = 200,
    Location = "Kuća",
    MuscleGroups = new() { "Svi mišići", "Fleksibilnost" }
};

// David - Aktivnost 2: Čitanje Tore
var davidTora = new SpiritualActivity(david.Id, 3, "Čitanje Tore")
{
    BookId = 3,
    Book = db.SpiritualBooks[2],
    CurrentPage = 50,
    PagesRead = 10,
    DurationMinutes = 35,
    Reflection = "Stara mudrost koja su još uvijek relevantna.",
    Difficulty = DifficultyLevel.Medium
};

// David - Aktivnost 3: Meditacija
var davidMeditacija = new Meditation(david.Id, "Vizualizacija", MeditationType.Visualization, 30)
{
    FocusArea = "Postizanje ciljeva",
    StressReliefScore = 7,
    MentalClarity = 8,
    Difficulty = DifficultyLevel.Hard
};

// David - Aktivnost 4: Dnevnik
var davidDnevnik = new DailyJournal(david.Id, "Dnevnik 24.03.2026")
{
    Mood = 7,
    EnergyLevel = 6
};
davidDnevnik.AddGoal("Yoga");
davidDnevnik.AddGoal("Meditacija");
davidDnevnik.AddAmbition("Naći mir i balans");
davidDnevnik.Reflection = "Dan je bio dobar, trebam više energije.";

david.AddActivity(davidFleksibilnost);
david.AddActivity(davidTora);
david.AddActivity(davidMeditacija);
david.AddJournal(davidDnevnik);

db.AddUser(david);

// ========== ISPIS STANJA ==========
Console.WriteLine($"✓ Kreirano {db.Users.Count} korisnika");
Console.WriteLine($"✓ Dostupno {db.SpiritualBooks.Count} duhovnih knjiga");
Console.WriteLine($"✓ Ukupno {db.GetAllActivities().Count} aktivnosti\n");

// ========== LINQ UPITI - LAB 1 ZAHTJEV ==========
Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
Console.WriteLine("║           LINQ UPITI NAD OBJEKTNIM MODELOM       ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

// LINQ 1: Pronađi sve korisnike po razini
Console.WriteLine("📊 LINQ 1: Korisnici sortirani po XP (descending)");
var usersByXP = db.Users.OrderByDescending(u => u.TotalXP).ToList();
foreach (var u in usersByXP)
    Console.WriteLine($"  • {u.Username} - {u.TotalXP} XP (Level {u.Level})");

// LINQ 2: Pronađi sve vježbe sa težinom Hard
Console.WriteLine("\n💪 LINQ 2: Sve vježbe sa težinom 'Hard'");
var hardExercises = db.GetActivitiesByType(ActivityType.Exercise)
    .Cast<Exercise>()
    .Where(e => e.Difficulty == DifficultyLevel.Hard)
    .ToList();
foreach (var e in hardExercises)
    Console.WriteLine($"  • {e.Title} ({e.ExerciseType})");

// LINQ 3: Pronađi najduliće meditacije
Console.WriteLine("\n🧘 LINQ 3: Top 3 najdulje meditacijske sesije");
var longestMeditations = db.GetActivitiesByType(ActivityType.Meditation)
    .Cast<Meditation>()
    .OrderByDescending(m => m.DurationMinutes)
    .Take(3)
    .ToList();
foreach (var m in longestMeditations)
    Console.WriteLine($"  • {m.Title} - {m.DurationMinutes} minuta");

// LINQ 4: Pronađi korisnike koji su čitali više od 10 stranica
Console.WriteLine("\n📖 LINQ 4: Aktivni čitatelji (pročitali 10+ stranica)");
var activeReaders = db.Users
    .Where(u => u.GetSpiritualActivities().Any(s => s.PagesRead >= 10))
    .Select(u => new { u.Username, PagesRead = u.GetSpiritualActivities().Sum(s => s.PagesRead) })
    .ToList();
foreach (var reader in activeReaders)
    Console.WriteLine($"  • {reader.Username} - {reader.PagesRead} stranica");

// LINQ 5: Pronađi prosječan XP po vrsti aktivnosti
Console.WriteLine("\n🎯 LINQ 5: Prosječan XP po vrsti aktivnosti");
var avgXpByType = db.GetAllActivities()
    .GroupBy(a => a.ActivityType)
    .Select(g => new { Type = g.Key, AvgXP = g.Average(a => a.CalculateXP()) })
    .ToList();
foreach (var item in avgXpByType)
    Console.WriteLine($"  • {item.Type}: {item.AvgXP:F1} XP");

// LINQ 6: Pronađi korisnike sa streakdom 
Console.WriteLine("\n🔥 LINQ 6: Korisnički streakovi");
var userStreaks = db.Users
    .Where(u => u.StreakDays > 0)
    .OrderByDescending(u => u.StreakDays)
    .Select(u => new { u.Username, u.StreakDays })
    .ToList();
if (userStreaks.Count > 0)
{
    foreach (var streak in userStreaks)
        Console.WriteLine($"  • {streak.Username} - {streak.StreakDays} dana");
}
else
    Console.WriteLine("  • Nema koristenika sa aktivnim streakovima");

// LINQ 7: Pronađi sve aktivnosti iz sadašnjeg dana
Console.WriteLine("\n📅 LINQ 7: Aktivnosti od danas");
var todayActivities = db.GetAllActivities()
    .Where(a => a.CompletedDate.Date == DateTime.Now.Date)
    .ToList();
foreach (var act in todayActivities)
    Console.WriteLine($"  • {act.Title} ({act.ActivityType})");

Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
Console.WriteLine("║             ASYNC DEMO - MEDITACIJA SIMULACIJA    ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

// ASYNC DEMO - simuliraj meditaciju
await SimulateMeditationAsync();


// ========== ASYNC METODA ==========
static async Task SimulateMeditationAsync()
{
    var meditation = new Meditation(2, "Vođena meditacija", MeditationType.Guided, 5);
    
    Console.WriteLine($"🧘 Započeta: {meditation.Title}\n");
    
    // Simuliraj faze meditacije
    string[] phases = { 
        "Sjedite opušteno i zatvorite oči...",
        "Udahnite duboko kroz nos (4 sekunde)...",
        "Zadržite dah (4 sekunde)...",
        "Izdahnite polako (4 sekunde)...",
        "Ponavljam ciklus..." 
    };

    for (int i = 0; i < 3; i++)
    {
        foreach (var phase in phases)
        {
            Console.WriteLine($"   {phase}");
            await Task.Delay(800); // Simulacija - čekaj 800ms
        }
    }

    meditation.StressReliefScore = 9;
    meditation.MentalClarity = 8;
    
    Console.WriteLine($"\n✓ Meditacija završena!");
    Console.WriteLine($"   Smirenje: {meditation.StressReliefScore}/10");
    Console.WriteLine($"   Jasnoća uma: {meditation.MentalClarity}/10\n");
}

