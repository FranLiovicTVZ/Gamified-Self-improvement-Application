using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GamefiedSelfImprovement;

/// <summary>
/// Predstavlja streak - broj dana za redom koji je korisnik obavljao aktivnosti
/// </summary>
public class Streak
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [ForeignKey("AppUser")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public virtual AppUser? AppUser { get; set; }

    [JsonPropertyName("currentStreak")]
    [Range(0, int.MaxValue, ErrorMessage = "Trenutni streak ne smije biti negativan")]
    public int CurrentStreak { get; set; } = 0;

    [JsonPropertyName("longestStreak")]
    [Range(0, int.MaxValue, ErrorMessage = "Najduži streak ne smije biti negativan")]
    public int LongestStreak { get; set; } = 0;

    [JsonPropertyName("lastActivityDate")]
    public DateTime? LastActivityDate { get; set; }

    [JsonPropertyName("lastStreakResetDate")]
    public DateTime? LastStreakResetDate { get; set; }

    [JsonPropertyName("totalActivitiesCompleted")]
    [Range(0, int.MaxValue, ErrorMessage = "Broj aktivnosti ne smije biti negativan")]
    public int TotalActivitiesCompleted { get; set; } = 0;

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedDate")]
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public Streak() { }

    public Streak(string userId)
    {
        UserId = userId;
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Ažurira streak kada je korisnik obavio aktivnost
    /// </summary>
    public void RecordActivity()
    {
        DateTime today = DateTime.UtcNow.Date;
        
        if (LastActivityDate == null)
        {
            // Prva aktivnost
            CurrentStreak = 1;
            LastActivityDate = today;
        }
        else if (LastActivityDate.Value.Date == today)
        {
            // Već je obavio aktivnost danas
            // Ništa se ne mijenja
        }
        else if (LastActivityDate.Value.Date == today.AddDays(-1))
        {
            // Aktivnost je obavio jučer - nastavi streak
            CurrentStreak++;
            LastActivityDate = today;
            
            // Ažuriraj longest streak ako je trebao
            if (CurrentStreak > LongestStreak)
            {
                LongestStreak = CurrentStreak;
            }
        }
        else
        {
            // Preskočio je dane - resetiraj streak
            CurrentStreak = 1;
            LastActivityDate = today;
            LastStreakResetDate = today;
        }

        TotalActivitiesCompleted++;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Resetira streak ako korisnik nije bio aktivna više od dana
    /// </summary>
    public bool CheckAndResetStreak()
    {
        if (LastActivityDate == null)
            return false;

        DateTime today = DateTime.UtcNow.Date;
        TimeSpan daysSinceLastActivity = today - LastActivityDate.Value.Date;

        if (daysSinceLastActivity.TotalDays > 1)
        {
            // Više od 1 dana je prošlo - resetiraj streak
            CurrentStreak = 0;
            LastStreakResetDate = today;
            UpdatedDate = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    public override string ToString() => 
        $"🔥 Streak: {CurrentStreak} dana | Najduži: {LongestStreak} | Ukupno aktivnosti: {TotalActivitiesCompleted}";
}
