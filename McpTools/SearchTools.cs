using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace GamefiedSelfImprovement.McpTools;

[McpServerToolType]
public sealed class SearchTools
{
    [McpServerTool, Description("Search across exercises, meditations, journals, and spiritual books by title, description, or author.")]
    public static async Task<string> Search(
        GamefiedSelfImprovementDbContext db,
        [Description("Search query string")] string query,
        [Description("Max results per category (default 5, max 20)")] int limit = 5)
    {
        limit = Math.Clamp(limit, 1, 20);
        var q = query.ToLower();

        var exercises = await db.Exercises
            .Where(e => e.Title.ToLower().Contains(q) || e.Description.ToLower().Contains(q))
            .Take(limit)
            .Select(e => new { e.Id, e.AppUserId, e.Title, Type = "Exercise", Detail = e.ExerciseType.ToString(), e.XpReward })
            .ToListAsync();

        var meditations = await db.Meditations
            .Where(m => m.Title.ToLower().Contains(q) || m.Description.ToLower().Contains(q))
            .Take(limit)
            .Select(m => new { m.Id, m.AppUserId, m.Title, Type = "Meditation", Detail = m.MeditationType.ToString(), m.XpReward })
            .ToListAsync();

        var books = await db.SpiritualBooks
            .Where(b => b.IsAvailable && (b.Title.ToLower().Contains(q) || b.Author.ToLower().Contains(q) || b.Description.ToLower().Contains(q)))
            .Take(limit)
            .Select(b => new { b.Id, AppUserId = (string?)null, b.Title, Type = "Book", Detail = b.BookType.ToString(), XpReward = 0 })
            .ToListAsync();

        return JsonSerializer.Serialize(new
        {
            query,
            totalFound = exercises.Count + meditations.Count + books.Count,
            exercises,
            meditations,
            books
        });
    }
}
