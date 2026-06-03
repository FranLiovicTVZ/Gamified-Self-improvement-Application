using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GamefiedSelfImprovement;

/// <summary>
/// Predstavlja duhovnu knjigu sa pratećim podacima (5+ svojstava)
/// </summary>
public class SpiritualBook
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("bookType")]
    public SpiritualBookType BookType { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = "Croatian";

    [JsonPropertyName("chapters")]
    public List<string> Chapters { get; set; } = new();

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; } = true;

    public SpiritualBook() { }

    public SpiritualBook(int id, string title, SpiritualBookType bookType, int totalPages)
    {
        Id = id;
        Title = title;
        BookType = bookType;
        TotalPages = totalPages;
    }

    public override string ToString() => $"{Title} ({BookType}) - {TotalPages} stranica";
}

/// <summary>
/// Bazna klasa za sve aktivnosti - client side validacija
/// </summary>
public abstract class Activity
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [ForeignKey("AppUser")]
    [JsonPropertyName("appUserId")]
    public string? AppUserId { get; set; }

    public virtual AppUser? AppUser { get; set; }

    // Zadržavam za backward compatibility sa starim User modelom
    [ForeignKey("User")]
    [JsonPropertyName("userId")]
    public int? UserId { get; set; }

    public virtual User? User { get; set; }

    [Required(ErrorMessage = "Naslov je obavezan")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Naslov mora biti između 3 i 200 znakova")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Opis ne smije biti duži od 1000 znakova")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("completedDate")]
    public DateTime CompletedDate { get; set; } = DateTime.Now;

    [JsonPropertyName("xpReward")]
    [Range(0, 10000, ErrorMessage = "XP mora biti između 0 i 10000")]
    public int XpReward { get; set; }

    [JsonPropertyName("activityType")]
    public ActivityType ActivityType { get; set; }

    [JsonPropertyName("difficulty")]
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    public Activity() { }

    public Activity(int userId, string title, ActivityType type)
    {
        UserId = userId;
        Title = title;
        ActivityType = type;
    }

    public Activity(string appUserId, string title, ActivityType type)
    {
        AppUserId = appUserId;
        Title = title;
        ActivityType = type;
    }

    public abstract int CalculateXP();
}

/// <summary>
/// Predstavlja vježbu u teretani ili vježbanje uopće (5+ svojstava)
/// </summary>
public class Exercise : Activity
{
    [JsonPropertyName("exerciseType")]
    public ExerciseType ExerciseType { get; set; }

    [Required(ErrorMessage = "Trajanje je obavezno")]
    [Range(1, 480, ErrorMessage = "Trajanje mora biti između 1 i 480 minuta")]
    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [Range(0, 5000, ErrorMessage = "Kalorije moraju biti između 0 i 5000")]
    [JsonPropertyName("caloriesBurned")]
    public int CaloriesBurned { get; set; }

    [Range(0, 100, ErrorMessage = "Setovi moraju biti između 0 i 100")]
    [JsonPropertyName("sets")]
    public int Sets { get; set; }

    [Range(0, 100, ErrorMessage = "Ponavljanja moraju biti između 0 i 100")]
    [JsonPropertyName("reps")]
    public int Reps { get; set; }

    [Range(0, 500, ErrorMessage = "Težina mora biti između 0 i 500 kg")]
    [JsonPropertyName("weight")]
    public decimal Weight { get; set; } // kg

    [JsonPropertyName("muscleGroups")]
    public List<string> MuscleGroups { get; set; } = new();

    [StringLength(100, ErrorMessage = "Lokacija ne smije biti duža od 100 znakova")]
    [JsonPropertyName("location")]
    public string Location { get; set; } = "Kuća"; // "Teretana", "Park", itd

    public Exercise()
    {
        ActivityType = ActivityType.Exercise;
    }

    public Exercise(int userId, string title, ExerciseType type)
        : base(userId, title, ActivityType.Exercise)
    {
        ExerciseType = type;
    }

    public override int CalculateXP()
    {
        // XP = duration * difficulty * exercise_multiplier
        int baseXP = DurationMinutes * (int)Difficulty + (Sets * Reps) / 2;
        return baseXP;
    }

    public override string ToString() => 
        $"💪 {Title}: {ExerciseType} - {DurationMinutes}min, {Sets}x{Reps} - {Weight}kg ({Difficulty})";
}

/// <summary>
/// Predstavlja čitanje religijskih tekstova (5+ svojstava)
/// </summary>
public class SpiritualActivity : Activity
{
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }

    [JsonPropertyName("book")]
    public SpiritualBook? Book { get; set; }

    [JsonPropertyName("pagesRead")]
    public int PagesRead { get; set; }

    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("reflection")]
    public string Reflection { get; set; } = string.Empty;

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    public SpiritualActivity()
    {
        ActivityType = ActivityType.Spiritual;
    }

    public SpiritualActivity(int userId, int bookId, string title)
        : base(userId, title, ActivityType.Spiritual)
    {
        BookId = bookId;
    }

    public override int CalculateXP()
    {
        // XP = pages_read * 2 + duration/10 + reflection_bonus
        int xp = (PagesRead * 2) + (DurationMinutes / 10);
        if (!string.IsNullOrEmpty(Reflection))
            xp += 10;
        return xp;
    }

    public override string ToString() => 
        $"📖 {Title}: Stranica {CurrentPage}/{Book?.TotalPages} ({Difficulty})";
}

/// <summary>
/// Predstavlja meditacijsku sesiju (5+ svojstava)
/// </summary>
public class Meditation : Activity
{
    [JsonPropertyName("meditationType")]
    public MeditationType MeditationType { get; set; }

    [Required(ErrorMessage = "Trajanje je obavezno")]
    [Range(1, 300, ErrorMessage = "Trajanje mora biti između 1 i 300 minuta")]
    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [StringLength(500, ErrorMessage = "Putanja do audio datoteke ne smije biti duža od 500 znakova")]
    [JsonPropertyName("audioFilePath")]
    public string AudioFilePath { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Fokus ne smije biti duži od 100 znakova")]
    [JsonPropertyName("focusArea")]
    public string FocusArea { get; set; } = "Opća svjesnost"; // "Mirovanje", "Emocije", itd

    [Range(1, 10, ErrorMessage = "Razina smirenja mora biti između 1 i 10")]
    [JsonPropertyName("stressReliefScore")]
    public int StressReliefScore { get; set; } // 1-10

    [Range(1, 10, ErrorMessage = "Jasnoća uma mora biti između 1 i 10")]
    [JsonPropertyName("mentalClarity")]
    public int MentalClarity { get; set; } // 1-10

    [StringLength(500, ErrorMessage = "Bilješke ne smiju biti duže od 500 znakova")]
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("completedDate")]
    public new DateTime CompletedDate { get; set; } = DateTime.Now;

    public Meditation()
    {
        ActivityType = ActivityType.Meditation;
    }

    public Meditation(int userId, string title, MeditationType type, int duration)
        : base(userId, title, ActivityType.Meditation)
    {
        MeditationType = type;
        DurationMinutes = duration;
    }

    public override int CalculateXP()
    {
        // XP = duration + stress relief score + mental clarity
        int xp = DurationMinutes + (StressReliefScore * 2) + (MentalClarity * 2);
        return xp;
    }

    public override string ToString() => 
        $"🧘 {Title}: {MeditationType} ({DurationMinutes}min) - Mir: {StressReliefScore}/10";
}

/// <summary>
/// Predstavlja dnevni dnevnik sa ciljevima i ambicijama (5+ svojstava)
/// </summary>
public class DailyJournal : Activity
{
    [JsonPropertyName("journalDate")]
    public DateTime JournalDate { get; set; } = DateTime.Now;

    [JsonPropertyName("dailyGoals")]
    public List<string> DailyGoals { get; set; } = new();

    [JsonPropertyName("ambitions")]
    public List<string> Ambitions { get; set; } = new();

    [JsonPropertyName("accomplishments")]
    public List<string> Accomplishments { get; set; } = new();

    [StringLength(2000, ErrorMessage = "Refleksija ne smije biti duža od 2000 znakova")]
    [JsonPropertyName("reflection")]
    public string Reflection { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Raspoloženje mora biti između 1 i 10")]
    [JsonPropertyName("mood")]
    public int Mood { get; set; } // 1-10 scale

    [Range(1, 10, ErrorMessage = "Razina energije mora biti između 1 i 10")]
    [JsonPropertyName("energyLevel")]
    public int EnergyLevel { get; set; } // 1-10 scale

    [JsonPropertyName("challenges")]
    public List<string> Challenges { get; set; } = new();

    public DailyJournal()
    {
        ActivityType = ActivityType.Journal;
    }

    public DailyJournal(int userId, string title)
        : base(userId, title, ActivityType.Journal)
    {
        JournalDate = DateTime.Now;
    }

    public override int CalculateXP()
    {
        // XP = goals_count * 3 + mood/2 + reflection_length/10
        int xp = (DailyGoals.Count * 3) + (Mood / 2) + (Reflection.Length / 10);
        return xp;
    }

    public void AddGoal(string goal) => DailyGoals.Add(goal);
    public void AddAmbition(string ambition) => Ambitions.Add(ambition);
    public void AddAccomplishment(string accomplishment) => Accomplishments.Add(accomplishment);

    public override string ToString() => 
        $"📝 Dnevnik {JournalDate:dd.MM.yyyy}: Raspoloženje: {Mood}/10, Ciljeva: {DailyGoals.Count}";
}
