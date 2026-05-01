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
/// Bazna klasa za sve aktivnosti
/// </summary>
public abstract class Activity
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [ForeignKey("User")]
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    public virtual User? User { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("completedDate")]
    public DateTime CompletedDate { get; set; } = DateTime.Now;

    [JsonPropertyName("xpReward")]
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

    public abstract int CalculateXP();
}

/// <summary>
/// Predstavlja vježbu u teretani ili vježbanje uopće (5+ svojstava)
/// </summary>
public class Exercise : Activity
{
    [JsonPropertyName("exerciseType")]
    public ExerciseType ExerciseType { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("caloriesBurned")]
    public int CaloriesBurned { get; set; }

    [JsonPropertyName("sets")]
    public int Sets { get; set; }

    [JsonPropertyName("reps")]
    public int Reps { get; set; }

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; } // kg

    [JsonPropertyName("muscleGroups")]
    public List<string> MuscleGroups { get; set; } = new();

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

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("audioFilePath")]
    public string AudioFilePath { get; set; } = string.Empty;

    [JsonPropertyName("focusArea")]
    public string FocusArea { get; set; } = "Opća svjesnost"; // "Mirovanje", "Emocije", itd

    [JsonPropertyName("stressReliefScore")]
    public int StressReliefScore { get; set; } // 1-10

    [JsonPropertyName("mentalClarity")]
    public int MentalClarity { get; set; } // 1-10

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

    [JsonPropertyName("reflection")]
    public string Reflection { get; set; } = string.Empty;

    [JsonPropertyName("mood")]
    public int Mood { get; set; } // 1-10 scale

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
