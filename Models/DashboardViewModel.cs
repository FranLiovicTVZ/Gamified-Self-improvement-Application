using GamefiedSelfImprovement;

namespace Gamified_Self_Improvement.Models;

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
}
