using Gamified_Self_Improvement.Models;
using Gamified_Self_Improvement.Repositories;
using Gamified_Self_Improvement.Services;
using GamefiedSelfImprovement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamified_Self_Improvement.Controllers;

/// <summary>
/// Home controller - admin i korisnički dashboard
/// </summary>
[Route("")]
[Route("home")]
public class HomeController : Controller
{
    private readonly UserRepository _userRepository;
    private readonly ActivityRepository _activityRepository;
    private readonly GamefiedSelfImprovementDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(
        UserRepository userRepository,
        ActivityRepository activityRepository,
        GamefiedSelfImprovementDbContext dbContext,
        UserManager<AppUser> userManager)
    {
        _userRepository = userRepository;
        _activityRepository = activityRepository;
        _dbContext = dbContext;
        _userManager = userManager;
    }

    /// <summary>
    /// Preusmjeri na odgovarajući dashboard ovisno o ulozi
    /// </summary>
    [Route("")]
    [Route("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (User.IsInRole("Admin"))
        {
            return RedirectToAction(nameof(AdminDashboard));
        }

        return RedirectToAction(nameof(UserDashboard));
    }

    /// <summary>
    /// Admin dashboard - pregled cijelog sustava
    /// </summary>
    [Route("admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        var appUsers = await _userManager.Users
            .OrderByDescending(u => u.TotalXP)
            .ToListAsync();

        var allActivities = _activityRepository.GetAll();

        var model = new AdminDashboardViewModel
        {
            TotalAppUsers = appUsers.Count,
            TotalLegacyUsers = _userRepository.GetAll().Count,
            TotalActivities = allActivities.Count,
            AppUsers = appUsers,
            RecentActivities = allActivities.OrderByDescending(a => a.CompletedDate).Take(8).ToList(),
            TopAppUser = appUsers.FirstOrDefault()
        };

        return View(model);
    }

    /// <summary>
    /// Korisnički dashboard - osobni pregled
    /// </summary>
    [Route("moj-dashboard")]
    [Authorize]
    public async Task<IActionResult> UserDashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (User.IsInRole("Admin"))
        {
            return RedirectToAction(nameof(AdminDashboard));
        }

        var streak = await _dbContext.Streaks.FirstOrDefaultAsync(s => s.UserId == user.Id);
        var userActivitiesQuery = _dbContext.Activities.Where(a => a.AppUserId == user.Id);
        var activities = await userActivitiesQuery
            .OrderByDescending(a => a.CompletedDate)
            .Take(6)
            .ToListAsync();

        var streakDays = streak?.CurrentStreak ?? 0;
        var model = new UserDashboardViewModel
        {
            User = user,
            Streak = streak,
            RecentActivities = activities,
            TotalActivities = await userActivitiesQuery.CountAsync(),
            ExerciseBadge   = BadgeCalculator.Calculate(user.ExerciseXP, streakDays),
            MeditationBadge = BadgeCalculator.Calculate(user.MeditationXP, streakDays),
            JournalBadge    = BadgeCalculator.Calculate(user.JournalXP, streakDays)
        };

        return View(model);
    }

    /// <summary>
    /// Leaderboard — svi korisnici poredani po XP-u, trenutni korisnik označen
    /// </summary>
    [Route("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> Leaderboard()
    {
        var adminEmails = await GetAdminEmailsAsync();
        var allUsers = _userRepository.GetAll()
            .Where(u => !adminEmails.Contains(u.Email))
            .OrderByDescending(u => u.TotalXP)
            .ThenByDescending(u => u.Level)
            .ToList();

        var currentUserEmail = User.Identity?.IsAuthenticated == true
            ? (await _userManager.GetUserAsync(User))?.Email
            : null;

        var entries = allUsers.Select((u, i) => new LeaderboardEntry
        {
            Rank          = i + 1,
            Username      = u.Username,
            UserId        = u.Id,
            TotalXP       = u.TotalXP,
            Level         = u.Level,
            StreakDays    = u.StreakDays,
            ExerciseBadge   = BadgeCalculator.Calculate(u.Activities.OfType<Exercise>().Sum(a => a.XpReward), u.StreakDays),
            MeditationBadge = BadgeCalculator.Calculate(u.Activities.OfType<Meditation>().Sum(a => a.XpReward), u.StreakDays),
            JournalBadge    = BadgeCalculator.Calculate(u.Journals.Sum(j => j.XpReward), u.StreakDays),
            IsCurrentUser = currentUserEmail != null &&
                            u.Email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase)
        }).ToList();

        return View(entries);
    }

    private async Task<HashSet<string>> GetAdminEmailsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        return admins
            .Where(a => a.Email != null)
            .Select(a => a.Email!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
