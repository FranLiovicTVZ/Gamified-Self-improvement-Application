using System.ComponentModel;
using System.Text.Json;
using Gamified_Self_Improvement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace GamefiedSelfImprovement.McpTools;

[McpServerToolType]
public sealed class UserTools
{
    [McpServerTool, Description("List all registered users with their XP, level, and streak days. Use the returned 'Id' as appUserId in other tools.")]
    public static async Task<string> ListUsers(
        UserManager<AppUser> userManager)
    {
        var users = await userManager.Users
            .OrderByDescending(u => u.TotalXP)
            .Take(50)
            .ToListAsync();

        return JsonSerializer.Serialize(users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.TotalXP,
            u.Level,
            u.StreakDays,
            PreferredMeditationType = u.PreferredMeditationType.ToString(),
            u.LastActiveDate
        }));
    }

    [McpServerTool, Description("Get detailed stats for a user: XP breakdown by category, level, activity counts, streak, and badge tier.")]
    public static async Task<string> GetUserStats(
        UserManager<AppUser> userManager,
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity string Id (get from list_users)")] string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return JsonSerializer.Serialize(new { error = $"User '{userId}' not found. Use list_users to get valid Ids." });

        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        var exerciseCount = await db.Exercises.CountAsync(e => e.AppUserId == userId);
        var meditationCount = await db.Meditations.CountAsync(m => m.AppUserId == userId);
        var journalCount = await db.DailyJournals.CountAsync(j => j.AppUserId == userId);

        var badgeTier = BadgeCalculator.Calculate(user.TotalXP, streak?.CurrentStreak ?? 0);

        return JsonSerializer.Serialize(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.TotalXP,
            user.Level,
            xpByCategory = new
            {
                exercise = user.ExerciseXP,
                meditation = user.MeditationXP,
                journal = user.JournalXP
            },
            streak = new
            {
                current = streak?.CurrentStreak ?? 0,
                longest = streak?.LongestStreak ?? 0,
                totalActivities = streak?.TotalActivitiesCompleted ?? 0,
                lastActivity = streak?.LastActivityDate
            },
            activityCounts = new { exerciseCount, meditationCount, journalCount },
            badge = new
            {
                tier = badgeTier.ToString(),
                emoji = BadgeCalculator.GetEmoji(badgeTier),
                label = BadgeCalculator.GetLabel(badgeTier)
            }
        });
    }

    [McpServerTool, Description("Get a user's profile: bio, preferences, badge, and favorite books.")]
    public static async Task<string> GetUserProfile(
        UserManager<AppUser> userManager,
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity string Id (get from list_users)")] string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return JsonSerializer.Serialize(new { error = $"User '{userId}' not found. Use list_users to get valid Ids." });

        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        var badge = BadgeCalculator.Calculate(user.TotalXP, streak?.CurrentStreak ?? 0);

        return JsonSerializer.Serialize(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.Bio,
            user.TotalXP,
            user.Level,
            user.CreatedDate,
            PreferredMeditationType = user.PreferredMeditationType.ToString(),
            user.FavoriteBooks,
            badge = new
            {
                tier = badge.ToString(),
                emoji = BadgeCalculator.GetEmoji(badge),
                label = BadgeCalculator.GetLabel(badge),
                color = BadgeCalculator.GetColor(badge)
            }
        });
    }
}
