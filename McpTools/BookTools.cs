using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace GamefiedSelfImprovement.McpTools;

[McpServerToolType]
public sealed class BookTools
{
    [McpServerTool, Description("List available spiritual books. Optionally filter by type.")]
    public static async Task<string> ListBooks(
        GamefiedSelfImprovementDbContext db,
        [Description("Book type filter: Bible, Quran, Torah, BuddhiticScriptures, Hindu (optional)")] string? type = null)
    {
        var query = db.SpiritualBooks.Where(b => b.IsAvailable);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<SpiritualBookType>(type, true, out var bookType))
            query = query.Where(b => b.BookType == bookType);

        var books = await query.OrderBy(b => b.Title).ToListAsync();

        return JsonSerializer.Serialize(books.Select(b => new
        {
            b.Id,
            b.Title,
            b.Author,
            BookType = b.BookType.ToString(),
            b.TotalPages,
            b.Description,
            b.Language
        }));
    }
}
