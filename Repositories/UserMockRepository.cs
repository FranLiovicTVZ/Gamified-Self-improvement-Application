using GamefiedSelfImprovement;

namespace Gamified_Self_Improvement.Repositories;

/// <summary>
/// Mock repository za korisnike - sprema sve korisnike s Lab 1 podatcima
/// </summary>
public class UserMockRepository
{
    private readonly List<User> _users;

    public UserMockRepository()
    {
        _users = InitializeMockUsers();
    }

    /// <summary>
    /// Vraća sve korisnike
    /// </summary>
    public List<User> GetAll()
    {
        return _users.ToList();
    }

    /// <summary>
    /// Dohvata korisnika po ID-u
    /// </summary>
    public User? GetById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    private List<User> InitializeMockUsers()
    {
        var users = new List<User>();

        // KORISNIK 1 - Marko92
        var marko = new User(1, "Marko92", "marko@example.com")
        {
            Bio = "Fitness entuzijast, čita Bibliju",
            PreferredMeditationType = MeditationType.Breathing,
            TotalXP = 350,
            Level = 2
        };

        // Marko - vježbe
        var markoVjezba = new Exercise(marko.Id, "Bench Press sesija", ExerciseType.Strength)
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

        var markoChitanje = new SpiritualActivity(marko.Id, 1, "Čitanje Biblije")
        {
            BookId = 1,
            CurrentPage = 45,
            PagesRead = 15,
            DurationMinutes = 30,
            Reflection = "Duboka poruka o vjeri i naději.",
            Difficulty = DifficultyLevel.Medium
        };

        var markoMeditacija = new Meditation(marko.Id, "Jutarnja meditacija", MeditationType.Breathing, 20)
        {
            FocusArea = "Smirenje uma",
            StressReliefScore = 8,
            MentalClarity = 7,
            Difficulty = DifficultyLevel.Easy,
            Notes = "Osjećam se mirnije i fokusiranije"
        };

        marko.AddActivity(markoVjezba);
        marko.AddActivity(markoChitanje);
        marko.AddActivity(markoMeditacija);

        // Marko - dnevnik
        var markoDnevnik = new DailyJournal(marko.Id, "Dnevnik 24.03.2026")
        {
            Mood = 8,
            EnergyLevel = 7
        };
        markoDnevnik.AddGoal("Završiti teretanu");
        markoDnevnik.AddGoal("Pročitati 20 stranica Biblije");
        markoDnevnik.AddAmbition("Postati jači");
        markoDnevnik.Reflection = "Odličan dan, motiviran za nastavak.";
        marko.AddJournal(markoDnevnik);

        users.Add(marko);

        // KORISNIK 2 - AminaX
        var amina = new User(2, "AminaX", "amina@example.com")
        {
            Bio = "Islamska vjera, duhovni razvitak",
            PreferredMeditationType = MeditationType.Mantras,
            TotalXP = 420,
            Level = 3
        };

        var aminaJog = new Exercise(amina.Id, "Jog u parku", ExerciseType.Cardio)
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

        var aminaKuran = new SpiritualActivity(amina.Id, 2, "Čitanje Kurana")
        {
            BookId = 2,
            CurrentPage = 120,
            PagesRead = 20,
            DurationMinutes = 45,
            Reflection = "Muhammadove poruke su vječne i relevantne.",
            Difficulty = DifficultyLevel.Hard
        };

        var aminaMeditacija = new Meditation(amina.Id, "Noćna meditacija", MeditationType.Mindfulness, 25)
        {
            FocusArea = "Razumijevanje sebe",
            StressReliefScore = 9,
            MentalClarity = 8,
            Difficulty = DifficultyLevel.Medium
        };

        amina.AddActivity(aminaJog);
        amina.AddActivity(aminaKuran);
        amina.AddActivity(aminaMeditacija);

        var aminaDnevnik = new DailyJournal(amina.Id, "Dnevnik 24.03.2026")
        {
            Mood = 9,
            EnergyLevel = 8
        };
        aminaDnevnik.AddGoal("Završiti jog");
        aminaDnevnik.AddGoal("Pročitati 20 stranica Kurana");
        aminaDnevnik.AddAmbition("Biti bolji čovjek");
        aminaDnevnik.Reflection = "Osjećam veću duhovnu povezanost.";
        amina.AddJournal(aminaDnevnik);

        users.Add(amina);

        // KORISNIK 3 - DavidT
        var david = new User(3, "DavidT", "david@example.com")
        {
            Bio = "Korisnik Jude, brojanje kalorijaa",
            PreferredMeditationType = MeditationType.Visualization,
            TotalXP = 280,
            Level = 2
        };

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

        var davidTora = new SpiritualActivity(david.Id, 3, "Čitanje Tore")
        {
            BookId = 3,
            CurrentPage = 50,
            PagesRead = 10,
            DurationMinutes = 35,
            Reflection = "Stara mudrost koja su još uvijek relevantna.",
            Difficulty = DifficultyLevel.Medium
        };

        var davidMeditacija = new Meditation(david.Id, "Vizualizacija", MeditationType.Visualization, 30)
        {
            FocusArea = "Postizanje ciljeva",
            StressReliefScore = 7,
            MentalClarity = 8,
            Difficulty = DifficultyLevel.Hard
        };

        david.AddActivity(davidFleksibilnost);
        david.AddActivity(davidTora);
        david.AddActivity(davidMeditacija);

        var davidDnevnik = new DailyJournal(david.Id, "Dnevnik 24.03.2026")
        {
            Mood = 7,
            EnergyLevel = 6
        };
        davidDnevnik.AddGoal("Yoga");
        davidDnevnik.AddGoal("Meditacija");
        davidDnevnik.AddAmbition("Naći mir i balans");
        davidDnevnik.Reflection = "Dan je bio dobar, trebam više energije.";
        david.AddJournal(davidDnevnik);

        users.Add(david);

        return users;
    }
}
