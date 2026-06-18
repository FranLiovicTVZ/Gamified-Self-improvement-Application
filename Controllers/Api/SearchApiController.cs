using GamefiedSelfImprovement;
using GamefiedSelfImprovement.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamefiedSelfImprovement.Controllers.Api;

[Route("api/search")]
[AllowAnonymous]
public class SearchApiController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;

    private static readonly List<(string Title, string Subtitle, string Url, string Icon, string[] Roles)> Pages =
    [
        ("Dashboard",            "Početna stranica",               "/",                              "bi-speedometer2",   ["User", "Admin", "Manager"]),
        ("Aktivnosti",           "Pregled svih aktivnosti",        "/aktivnosti",                    "bi-lightning",      ["User", "Admin", "Manager"]),
        ("Nova vježba",          "Dodaj vježbu",                   "/aktivnosti/nova-vjezba",        "bi-bicycle",        ["User", "Admin", "Manager"]),
        ("Nova meditacija",      "Dodaj meditaciju",               "/aktivnosti/nova-meditacija",    "bi-moon-stars",     ["User", "Admin", "Manager"]),
        ("Novi dnevnik",         "Dodaj unos u dnevnik",           "/aktivnosti/novi-dnevnik",       "bi-journal-text",   ["User", "Admin", "Manager"]),
        ("Rang-lista",           "Ljestvica prema XP-u",           "/leaderboard",                   "bi-trophy",         ["User", "Admin", "Manager"]),
        ("Moj profil",           "Uredi profil",                   "/profil",                        "bi-person-circle",  ["User", "Admin", "Manager"]),
        ("Korisnici",            "Popis korisnika",                "/korisnici",                     "bi-people",         ["Admin", "Manager"]),
        ("Dodaj korisnika",      "Stvori novog korisnika",         "/korisnici/dodaj",               "bi-person-plus",    ["Admin"]),
        ("Admin Dashboard",      "Administratorski pregled",       "/home/admin/dashboard",          "bi-shield-check",   ["Admin"]),
    ];

    public SearchApiController(
        GamefiedSelfImprovementDbContext dbContext,
        UserManager<AppUser> userManager)
        : base(dbContext)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<GlobalSearchResultDTO>> Search(string q = "", int limit = 5)
    {
        q = q.Trim();
        if (q.Length < 2)
            return Ok(new GlobalSearchResultDTO { Query = q, Results = [], TotalCount = 0 });

        var results = new List<SearchItemDTO>();
        bool isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
        bool isAuthenticated = User.Identity?.IsAuthenticated == true;
        string? currentUserId = _userManager.GetUserId(User);

        // Stranice (in-memory — uvijek rade)
        foreach (var (title, subtitle, url, icon, roles) in Pages)
        {
            if (!title.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                !subtitle.Contains(q, StringComparison.OrdinalIgnoreCase))
                continue;

            bool allowed = !isAuthenticated
                ? roles.Contains("User")
                : roles.Any(r => User.IsInRole(r));

            if (!allowed) continue;

            results.Add(new SearchItemDTO
            {
                Title    = title,
                Subtitle = subtitle,
                Url      = url,
                Category = "Stranica",
                Icon     = icon
            });

            if (results.Count(r => r.Category == "Stranica") >= limit) break;
        }

        // Aktivnosti — Contains() se prevodi u SQL LIKE koji je case-insensitive na SQL Serveru
        var activityQuery = _dbContext.Activities.AsQueryable()
            .Where(a => a.Title.Contains(q) ||
                        (a.Description != null && a.Description.Contains(q)));

        if (!isAdminOrManager && isAuthenticated)
        {
            // Uključi i aktivnosti iz legacy sustava (UserId) i novog (AppUserId)
            int? legacyUserId = null;
            if (currentUserId != null)
            {
                var appUser = await _userManager.FindByIdAsync(currentUserId);
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
            .Take(limit)
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
            var users = await _userManager.Users
                .Where(u => (u.UserName != null && u.UserName.Contains(q)) ||
                            (u.Email    != null && u.Email.Contains(q)))
                .Take(limit)
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
            .Take(limit)
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

        return Ok(new GlobalSearchResultDTO
        {
            Query      = q,
            Results    = results,
            TotalCount = results.Count
        });
    }
}
