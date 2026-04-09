using GamefiedSelfImprovement;

namespace Gamified_Self_Improvement.Models;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalActivities { get; set; }
    public User? TopUser { get; set; }
    public List<Activity> RecentActivities { get; set; } = new();
    public List<User> Users { get; set; } = new();
}
