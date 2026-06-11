using GamefiedSelfImprovement;

namespace Gamified_Self_Improvement.Models;

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string Username { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int TotalXP { get; set; }
    public int Level { get; set; }
    public int StreakDays { get; set; }
    public BadgeTier ExerciseBadge { get; set; }
    public BadgeTier MeditationBadge { get; set; }
    public BadgeTier JournalBadge { get; set; }
    public bool IsCurrentUser { get; set; }
}

public class AdminDashboardViewModel
{
    public int TotalAppUsers { get; set; }
    public int TotalLegacyUsers { get; set; }
    public int TotalActivities { get; set; }
    public List<AppUser> AppUsers { get; set; } = new();
    public List<Activity> RecentActivities { get; set; } = new();
    public AppUser? TopAppUser { get; set; }
}

public class UserDashboardViewModel
{
    public AppUser User { get; set; } = null!;
    public Streak? Streak { get; set; }
    public List<Activity> RecentActivities { get; set; } = new();
    public int TotalActivities { get; set; }
    public BadgeTier ExerciseBadge { get; set; }
    public BadgeTier MeditationBadge { get; set; }
    public BadgeTier JournalBadge { get; set; }
}
