using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GamefiedSelfImprovement;

/// <summary>
/// Predstavlja XP nagradu za specifičnu aktivnost
/// </summary>
public class XPReward
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("xpAmount")]
    public int XpAmount { get; set; }

    [JsonPropertyName("activityType")]
    public ActivityType ActivityType { get; set; }

    [JsonPropertyName("unlockedDate")]
    public DateTime UnlockedDate { get; set; } = DateTime.Now;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "🏆";

    public XPReward() { }

    public XPReward(string name, int xpAmount, ActivityType type)
    {
        Name = name;
        XpAmount = xpAmount;
        ActivityType = type;
    }

    public override string ToString() => $"{Icon} {Name} (+{XpAmount} XP)";
}

/// <summary>
/// Predstavlja log treninga u teretani - detaljna evidencija (6 svojstava)
/// </summary>
public class TrainingLog
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("exerciseId")]
    public int ExerciseId { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("exerciseName")]
    public string ExerciseName { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; } // kg

    [JsonPropertyName("sets")]
    public int Sets { get; set; }

    [JsonPropertyName("reps")]
    public int Reps { get; set; }

    [JsonPropertyName("restSeconds")]
    public int RestSeconds { get; set; } = 60;

    [JsonPropertyName("logDate")]
    public DateTime LogDate { get; set; } = DateTime.Now;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    public TrainingLog() { }

    public TrainingLog(int userId, string exerciseName)
    {
        UserId = userId;
        ExerciseName = exerciseName;
    }

    public override string ToString() => 
        $"🏋️ {ExerciseName}: {Sets}x{Reps} @ {Weight}kg ({LogDate:dd.MM.yyyy})";
}

/// <summary>
/// Globalna baza podataka - sadrži sve korisnike, knjige, aktivnosti itd.
/// Koristi se za upravljanje relacijama između objekata
/// </summary>
public class GameDatabase
{
    [JsonPropertyName("users")]
    public List<User> Users { get; set; } = new();

    [JsonPropertyName("spiritualBooks")]
    public List<SpiritualBook> SpiritualBooks { get; set; } = new();

    [JsonPropertyName("xpRewards")]
    public List<XPReward> XpRewards { get; set; } = new();

    [JsonPropertyName("trainingLogs")]
    public List<TrainingLog> TrainingLogs { get; set; } = new();

    public GameDatabase()
    {
        InitializeSpiritualBooks();
        InitializeRewards();
    }

    /// <summary>
    /// Inicijalizuj dostupne duhovne knjige
    /// </summary>
    private void InitializeSpiritualBooks()
    {
        SpiritualBooks.Add(new SpiritualBook(1, "Biblija (Novi zavjet)", SpiritualBookType.Bible, 260)
        {
            Author = "Razni autori",
            Description = "Kršćanski sveti tekst",
            Language = "Croatian"
        });

        SpiritualBooks.Add(new SpiritualBook(2, "Kuran", SpiritualBookType.Quran, 604)
        {
            Author = "Prorok Muhammad (Mir budi s njim)",
            Description = "Islamski sveti tekst",
            Language = "Croatian"
        });

        SpiritualBooks.Add(new SpiritualBook(3, "Tora", SpiritualBookType.Torah, 187)
        {
            Author = "Mojsije i drugi",
            Description = "Židovski sveti tekst",
            Language = "Croatian"
        });
    }

    /// <summary>
    /// Inicijalizuj dostupne nagrade
    /// </summary>
    private void InitializeRewards()
    {
        XpRewards.Add(new XPReward("Početni trening", 50, ActivityType.Exercise) { Icon = "💪" });
        XpRewards.Add(new XPReward("Dnevna meditacija", 30, ActivityType.Meditation) { Icon = "🧘" });
        XpRewards.Add(new XPReward("Čitanje duhovnog teksta", 40, ActivityType.Spiritual) { Icon = "📖" });
        XpRewards.Add(new XPReward("Refleksija u dnevniku", 25, ActivityType.Journal) { Icon = "📝" });
    }

    public User? GetUserById(int id) => Users.FirstOrDefault(u => u.Id == id);

    public User? GetUserByUsername(string username) => Users.FirstOrDefault(u => u.Username == username);

    public SpiritualBook? GetBookById(int id) => SpiritualBooks.FirstOrDefault(b => b.Id == id);

    public List<Activity> GetAllActivities() => Users.SelectMany(u => u.Activities).ToList();

    public List<Activity> GetActivitiesByType(ActivityType type) => 
        GetAllActivities().Where(a => a.ActivityType == type).ToList();

    public TrainingLog? GetTrainingLogById(int id) => TrainingLogs.FirstOrDefault(t => t.Id == id);

    public void AddUser(User user) => Users.Add(user);

    public void AddTrainingLog(TrainingLog log) => TrainingLogs.Add(log);

    public override string ToString() => 
        $"📊 Database: {Users.Count} korisnika, {GetAllActivities().Count} aktivnosti, {SpiritualBooks.Count} knjiga";
}
