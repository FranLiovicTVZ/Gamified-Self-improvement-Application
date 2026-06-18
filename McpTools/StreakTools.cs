using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace GamefiedSelfImprovement.McpTools;

[McpServerToolType]
public sealed class StreakTools
{
    [McpServerTool, Description("Get the current and longest activity streak for a specific user.")]
    public static async Task<string> GetStreak(
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity string Id (get from list_users)")] string userId)
    {
        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        if (streak == null)
            return JsonSerializer.Serialize(new
            {
                userId,
                currentStreak = 0,
                longestStreak = 0,
                totalActivities = 0,
                lastActivity = (DateTime?)null
            });

        return JsonSerializer.Serialize(new
        {
            userId,
            streak.CurrentStreak,
            streak.LongestStreak,
            streak.TotalActivitiesCompleted,
            streak.LastActivityDate,
            streak.UpdatedDate
        });
    }

    [McpServerTool, Description("Get the top 10 users ranked by current activity streak (leaderboard).")]
    public static async Task<string> GetLeaderboard(
        GamefiedSelfImprovementDbContext db,
        UserManager<AppUser> userManager)
    {
        var streaks = await db.Streaks
            .OrderByDescending(s => s.CurrentStreak)
            .Take(10)
            .ToListAsync();

        var result = new List<object>();
        for (int i = 0; i < streaks.Count; i++)
        {
            var s = streaks[i];
            var user = await userManager.FindByIdAsync(s.UserId);
            result.Add(new
            {
                rank = i + 1,
                userId = s.UserId,
                userName = user?.UserName ?? "Unknown",
                s.CurrentStreak,
                s.LongestStreak,
                s.TotalActivitiesCompleted
            });
        }

        return JsonSerializer.Serialize(result);
    }
}
