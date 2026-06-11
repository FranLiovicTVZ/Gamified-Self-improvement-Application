using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GamefiedSelfImprovement;

/// <summary>
/// Proširena Identity korisnik klasa sa dodatnim poljima specifičnim za aplikaciju
/// </summary>
public class AppUser : IdentityUser
{
    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB mora biti točno 11 znakova")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve")]
    [JsonPropertyName("oib")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora biti točno 13 znakova")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadržavati samo brojeve")]
    [JsonPropertyName("jmbg")]
    public string JMBG { get; set; } = string.Empty;

    [JsonPropertyName("totalXP")]
    [Range(0, int.MaxValue, ErrorMessage = "XP ne smije biti negativan")]
    public int TotalXP { get; set; } = 0;

    [JsonPropertyName("level")]
    [Range(1, 100, ErrorMessage = "Nivo mora biti između 1 i 100")]
    public int Level { get; set; } = 1;

    [StringLength(500, ErrorMessage = "Bio ne smije biti duži od 500 znakova")]
    [JsonPropertyName("bio")]
    public string Bio { get; set; } = string.Empty;

    [JsonPropertyName("profileImagePath")]
    public string ProfileImagePath { get; set; } = string.Empty;

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("preferredMeditationType")]
    public MeditationType PreferredMeditationType { get; set; } = MeditationType.Mindfulness;

    [JsonPropertyName("favoriteBooks")]
    public List<int> FavoriteBooks { get; set; } = new(); // BookIds

    [JsonPropertyName("streakDays")]
    public int StreakDays { get; set; } = 0;

    [JsonPropertyName("lastActiveDate")]
    public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("exerciseXP")]
    public int ExerciseXP { get; set; } = 0;

    [JsonPropertyName("meditationXP")]
    public int MeditationXP { get; set; } = 0;

    [JsonPropertyName("journalXP")]
    public int JournalXP { get; set; } = 0;

    // Navigacijska svojstva
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public virtual ICollection<DailyJournal> Journals { get; set; } = new List<DailyJournal>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public virtual Streak? Streak { get; set; }

    public override string ToString() => 
        $"👤 {UserName} - Level {Level} ({TotalXP} XP) | OIB: {OIB}";
}
