using GamefiedSelfImprovement;
using GamefiedSelfImprovement.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamified_Self_Improvement.Controllers;

[Route("pretraga")]
public class SearchController : BaseController
{
    private readonly GamefiedSelfImprovementDbContext _dbContext;

    public SearchController(
        UserManager<AppUser> userManager,
        GamefiedSelfImprovementDbContext dbContext)
        : base(userManager)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(string q = "")
    {
        q = q.Trim();
        ViewBag.Query = q;

        if (q.Length < 2)
            return View(new GlobalSearchResultDTO { Query = q, Results = [], TotalCount = 0 });

        bool isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
        bool isAuthenticated = User.Identity?.IsAuthenticated == true;
        string? currentUserId = CurrentUserId;

        var results = new List<SearchItemDTO>();

        // Aktivnosti
        var activityQuery = _dbContext.Activities.AsQueryable()
            .Where(a => a.Title.Contains(q) ||
                        (a.Description != null && a.Description.Contains(q)));

        if (!isAdminOrManager && isAuthenticated)
        {
            int? legacyUserId = null;
            if (currentUserId != null)
            {
                var appUser = await UserManager.FindByIdAsync(currentUserId);
                if (appUser?.Email != null)
                {
                    legacyUserId = await _dbContext.Users
                        .Where(u => u.Email == appUser.Email)
                        .Select(u => (int?)u.Id)
                        .FirstOrDefaultAsync();
                }
            }

            activityQuery = activityQuery.Where(a =>
                a.AppUserId == currentUserId ||
                (legacyUserId != null && a.UserId == legacyUserId));
        }

        var activities = await activityQuery
            .OrderByDescending(a => a.CompletedDate)
            .Select(a => new { a.Id, a.Title, a.ActivityType })
            .ToListAsync();

        foreach (var a in activities)
        {
            results.Add(new SearchItemDTO
            {
                Title    = a.Title,
                Subtitle = a.ActivityType.ToString(),
                Url      = $"/aktivnosti/{a.Id}",
                Category = "Aktivnost",
                Icon     = a.ActivityType switch
                {
                    ActivityType.Exercise   => "bi-bicycle",
                    ActivityType.Meditation => "bi-moon-stars",
                    ActivityType.Journal    => "bi-journal-text",
                    _                       => "bi-star"
                }
            });
        }

        // Korisnici
        if (isAuthenticated)
        {
            var users = await UserManager.Users
                .Where(u => (u.UserName != null && u.UserName.Contains(q)) ||
                            (u.Email    != null && u.Email.Contains(q)))
                .Select(u => new { u.Id, u.UserName, u.Email, u.Level })
                .ToListAsync();

            foreach (var u in users)
            {
                results.Add(new SearchItemDTO
                {
                    Title    = u.UserName ?? u.Email ?? u.Id,
                    Subtitle = $"Razina {u.Level}",
                    Url      = $"/profil/user/{u.Id}",
                    Category = "Korisnik",
                    Icon     = "bi-person"
                });
            }
        }

        // Duhovne knjige
        var books = await _dbContext.SpiritualBooks
            .Where(b => b.IsAvailable &&
                       (b.Title.Contains(q) ||
                        (b.Author != null && b.Author.Contains(q))))
            .Select(b => new { b.Id, b.Title, b.Author })
            .ToListAsync();

        foreach (var b in books)
        {
            results.Add(new SearchItemDTO
            {
                Title    = b.Title,
                Subtitle = b.Author ?? "",
                Url      = $"/api/spiritual-books/{b.Id}",
                Category = "Knjiga",
                Icon     = "bi-book"
            });
        }

        var model = new GlobalSearchResultDTO
        {
            Query      = q,
            Results    = results,
            TotalCount = results.Count
        };

        return View(model);
    }
}
