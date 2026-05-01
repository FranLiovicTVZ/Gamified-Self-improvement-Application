using Gamified_Self_Improvement.Models;
using Gamified_Self_Improvement.Repositories;
using GamefiedSelfImprovement;
using Microsoft.AspNetCore.Mvc;

namespace Gamified_Self_Improvement.Controllers;

/// <summary>
/// Home controller - početna stranica i dashboard
/// </summary>
[Route("")]
[Route("home")]
public class HomeController : Controller
{
    private readonly UserRepository _userRepository;
    private readonly ActivityRepository _activityRepository;

    public HomeController(UserRepository userRepository, ActivityRepository activityRepository)
    {
        _userRepository = userRepository;
        _activityRepository = activityRepository;
    }

    /// <summary>
    /// Dashboard stranica
    /// URL: / ili /home ili /dashboard
    /// </summary>
    [Route("")]
    [Route("dashboard")]
    public IActionResult Dashboard()
    {
        var users = _userRepository.GetAll();
        var allActivities = _activityRepository.GetAll();

        var dashboardViewModel = new DashboardViewModel
        {
            TotalUsers = users.Count,
            TotalActivities = allActivities.Count,
            TopUser = users.OrderByDescending(u => u.TotalXP).FirstOrDefault(),
            RecentActivities = allActivities.OrderByDescending(a => a.CompletedDate).Take(5).ToList(),
            Users = users
        };

        return View(dashboardViewModel);
    }
}
