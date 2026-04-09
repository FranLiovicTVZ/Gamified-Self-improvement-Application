using Gamified_Self_Improvement.Models;
using Gamified_Self_Improvement.Repositories;
using GamefiedSelfImprovement;
using Microsoft.AspNetCore.Mvc;

namespace Gamified_Self_Improvement.Controllers;

public class HomeController : Controller
{
    private readonly UserMockRepository _userRepository;
    private readonly ActivityMockRepository _activityRepository;

    public HomeController(UserMockRepository userRepository, ActivityMockRepository activityRepository)
    {
        _userRepository = userRepository;
        _activityRepository = activityRepository;
    }

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
