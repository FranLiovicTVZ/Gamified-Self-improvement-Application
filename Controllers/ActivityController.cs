using Gamified_Self_Improvement.Repositories;
using GamefiedSelfImprovement;
using Microsoft.AspNetCore.Mvc;

namespace Gamified_Self_Improvement.Controllers;

/// <summary>
/// Controller za upravljanje aktivnostima - custom routing primjena
/// </summary>
[Route("aktivnosti")]
public class ActivityController : Controller
{
    private readonly ActivityRepository _activityRepository;
    private readonly UserRepository _userRepository;

    public ActivityController(ActivityRepository activityRepository, UserRepository userRepository)
    {
        _activityRepository = activityRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Lista svih aktivnosti
    /// URL: /aktivnosti ili /aktivnosti/po-korisniku/{userId}
    /// </summary>
    [Route("")]
    [Route("po-korisniku/{userId:int}")]
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
    /// URL: /aktivnosti/{id}
    /// </summary>
    [Route("{id:int}")]
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
