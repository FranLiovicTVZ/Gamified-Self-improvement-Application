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

    /// <summary>
    /// Forma za dodavanje nove vježbe - GET
    /// URL: /aktivnosti/nova-vjezba
    /// </summary>
    [Route("nova-vjezba")]
    [HttpGet]
    public IActionResult CreateExercise()
    {
        ViewBag.Users = _userRepository.GetAll();
        return View();
    }

    /// <summary>
    /// Spremi novu vježbu - POST
    /// URL: /aktivnosti/nova-vjezba
    /// </summary>
    [Route("nova-vjezba")]
    [HttpPost]
    public IActionResult CreateExercise(Exercise exercise)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = _userRepository.GetAll();
            return View(exercise);
        }

        try
        {
            exercise.ActivityType = ActivityType.Exercise;
            _activityRepository.Add(exercise);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Users = _userRepository.GetAll();
            ModelState.AddModelError("", $"Greška pri dodavanju vježbe: {ex.Message}");
            return View(exercise);
        }
    }

    /// <summary>
    /// Forma za dodavanje meditacije - GET
    /// URL: /aktivnosti/nova-meditacija
    /// </summary>
    [Route("nova-meditacija")]
    [HttpGet]
    public IActionResult CreateMeditation()
    {
        ViewBag.Users = _userRepository.GetAll();
        return View();
    }

    /// <summary>
    /// Spremi novu meditaciju - POST
    /// URL: /aktivnosti/nova-meditacija
    /// </summary>
    [Route("nova-meditacija")]
    [HttpPost]
    public IActionResult CreateMeditation(Meditation meditation)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = _userRepository.GetAll();
            return View(meditation);
        }

        try
        {
            meditation.ActivityType = ActivityType.Meditation;
            _activityRepository.Add(meditation);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Users = _userRepository.GetAll();
            ModelState.AddModelError("", $"Greška pri dodavanju meditacije: {ex.Message}");
            return View(meditation);
        }
    }

    /// <summary>
    /// Forma za dodavanje unosa u dnevnik - GET
    /// URL: /aktivnosti/novi-dnevnik
    /// </summary>
    [Route("novi-dnevnik")]
    [HttpGet]
    public IActionResult CreateJournal()
    {
        ViewBag.Users = _userRepository.GetAll();
        return View();
    }

    /// <summary>
    /// Spremi novi unos u dnevnik - POST
    /// URL: /aktivnosti/novi-dnevnik
    /// </summary>
    [Route("novi-dnevnik")]
    [HttpPost]
    public IActionResult CreateJournal(DailyJournal journal)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = _userRepository.GetAll();
            return View(journal);
        }

        try
        {
            journal.ActivityType = ActivityType.Journal;
            _activityRepository.Add(journal);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Users = _userRepository.GetAll();
            ModelState.AddModelError("", $"Greška pri dodavanju dnevnika: {ex.Message}");
            return View(journal);
        }
    }
}
