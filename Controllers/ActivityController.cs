using Gamified_Self_Improvement.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Gamified_Self_Improvement.Controllers;

public class ActivityController : Controller
{
    private readonly ActivityMockRepository _activityRepository;
    private readonly UserMockRepository _userRepository;

    public ActivityController(ActivityMockRepository activityRepository, UserMockRepository userRepository)
    {
        _activityRepository = activityRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Lista svih aktivnosti
    /// </summary>
    public IActionResult Index(int? userId = null)
    {
        var activities = userId.HasValue 
            ? _activityRepository.GetByUserId(userId.Value)
            : _activityRepository.GetAll();

        ViewBag.UserId = userId;
        if (userId.HasValue)
        {
            var user = _userRepository.GetById(userId.Value);
            ViewBag.UserName = user?.Username;
        }

        return View(activities);
    }

    /// <summary>
    /// Detalji o specifičnoj aktivnosti
    /// </summary>
    public IActionResult Details(int id)
    {
        var activity = _activityRepository.GetById(id);
        if (activity == null)
            return NotFound();

        var user = _userRepository.GetById(activity.UserId);
        ViewBag.UserName = user?.Username;

        return View(activity);
    }
}
