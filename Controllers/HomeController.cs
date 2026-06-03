using Gamified_Self_Improvement.Models;
using Gamified_Self_Improvement.Repositories;
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

        var model = new UserDashboardViewModel
        {
            User = user,
            Streak = streak,
            RecentActivities = activities,
            TotalActivities = await userActivitiesQuery.CountAsync()
        };

        return View(model);
    }
}
