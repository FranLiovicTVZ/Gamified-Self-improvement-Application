using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GamefiedSelfImprovement;

/// <summary>
/// Predstavlja korisnika aplikacije - KOMPLEKSNA KLASA (8 svojstava) (1-N odnos s Activity)
/// </summary>
public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [JsonPropertyName("totalXP")]
    public int TotalXP { get; set; } = 0;

    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;

    [JsonPropertyName("bio")]
    public string Bio { get; set; } = string.Empty;

    [JsonPropertyName("profileImagePath")]
    public string ProfileImagePath { get; set; } = string.Empty;

    // 1-N relationship s Activity-ima
    [JsonPropertyName("activities")]
    public List<Activity> Activities { get; set; } = new();

    // 1-N relationship s DailyJournal-ima
    [JsonPropertyName("journals")]
    public List<DailyJournal> Journals { get; set; } = new();

    // Preferencijes korisnika
    [JsonPropertyName("preferredMeditationType")]
    public MeditationType PreferredMeditationType { get; set; } = MeditationType.Mindfulness;

    [JsonPropertyName("favoriteBooks")]
    public List<int> FavoriteBooks { get; set; } = new(); // BookIds

    [JsonPropertyName("streakDays")]
    public int StreakDays { get; set; } = 0;

    [JsonPropertyName("lastActiveDate")]
    public DateTime LastActiveDate { get; set; } = DateTime.Now;

    public User() { }

    public User(int id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
        CreatedDate = DateTime.Now;
    }

    /// <summary>
    /// Dodaj novu aktivnost korisniku
    /// </summary>
    public void AddActivity(Activity activity)
    {
        activity.UserId = this.Id;
        Activities.Add(activity);
        TotalXP += activity.CalculateXP();
        UpdateLevel();
        LastActiveDate = DateTime.Now;
        UpdateStreak();
    }

    /// <summary>
    /// Dodaj novi dnevnik
    /// </summary>
    public void AddJournal(DailyJournal journal)
    {
        journal.UserId = this.Id;
        Journals.Add(journal);
        TotalXP += journal.CalculateXP();
        UpdateLevel();
        LastActiveDate = DateTime.Now;
    }

    /// <summary>
    /// Izračunaj level na temelju XP-a
    /// </summary>
    private void UpdateLevel()
    {
        // 1000 XP po level-u
        Level = (TotalXP / 1000) + 1;
    }

    /// <summary>
    /// Ažuriraj streak - ako je aktivnost ista kao jučer, povećaj streak
    /// </summary>
    private void UpdateStreak()
    {
        var yesterday = DateTime.Now.AddDays(-1).Date;
        var hasActivityYesterday = Activities.Any(a => a.CompletedDate.Date == yesterday);

        if (hasActivityYesterday)
        {
            StreakDays++;
        }
        else if (DateTime.Now.Date > LastActiveDate.Date)
        {
            StreakDays = 0;
        }
    }

    /// <summary>
    /// Preuzmi sve vježbe korisnika
    /// </summary>
    public List<Exercise> GetExercises() => 
        Activities.OfType<Exercise>().ToList();

    /// <summary>
    /// Preuzmi sve duhovne aktivnosti
    /// </summary>
    public List<SpiritualActivity> GetSpiritualActivities() => 
        Activities.OfType<SpiritualActivity>().ToList();

    /// <summary>
    /// Preuzmi sve meditacijske sesije
    /// </summary>
    public List<Meditation> GetMeditations() => 
        Activities.OfType<Meditation>().ToList();

    /// <summary>
    /// Preuzmi ukupno vrijeme provedeno u vježbanju
    /// </summary>
    public int GetTotalExerciseMinutes()
    {
        return GetExercises().Sum(e => e.DurationMinutes);
    }

    /// <summary>
    /// Preuzmi najčešću vrstu vježbe
    /// </summary>
    public ExerciseType? GetFavoriteExerciseType()
    {
        var exercises = GetExercises();
        if (exercises.Count == 0) return null;
        return exercises.GroupBy(e => e.ExerciseType)
                       .OrderByDescending(g => g.Count())
                       .First()
                       .Key;
    }

    /// <summary>
    /// Preuzmi prosjek raspoloženja
    /// </summary>
    public double GetAverageMood()
    {
        if (Journals.Count == 0) return 0;
        return Journals.Average(j => j.Mood);
    }

    public override string ToString() => 
        $"👤 {Username} - Level {Level} ({TotalXP} XP) | Streak: {StreakDays} dana";
}
