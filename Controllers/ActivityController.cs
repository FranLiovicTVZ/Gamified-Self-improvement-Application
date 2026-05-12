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
            // Izračunaj XP ako nije postavljen
            if (exercise.XpReward == 0)
            {
                exercise.XpReward = exercise.CalculateXP();
            }
            _activityRepository.Add(exercise);
            
            // Ažuriraj XP korisnika
            var user = _userRepository.GetById(exercise.UserId);
            if (user != null)
            {
                user.TotalXP += exercise.XpReward;
                _userRepository.Update(user);
            }
            
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
            // Izračunaj XP ako nije postavljen
            if (meditation.XpReward == 0)
            {
                meditation.XpReward = meditation.CalculateXP();
            }
            _activityRepository.Add(meditation);
            
            // Ažuriraj XP korisnika
            var user = _userRepository.GetById(meditation.UserId);
            if (user != null)
            {
                user.TotalXP += meditation.XpReward;
                _userRepository.Update(user);
            }
            
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
            // Izračunaj XP ako nije postavljen
            if (journal.XpReward == 0)
            {
                journal.XpReward = journal.CalculateXP();
            }
            _activityRepository.Add(journal);
            
            // Ažuriraj XP korisnika
            var user = _userRepository.GetById(journal.UserId);
            if (user != null)
            {
                user.TotalXP += journal.XpReward;
                _userRepository.Update(user);
            }
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Users = _userRepository.GetAll();
            ModelState.AddModelError("", $"Greška pri dodavanju dnevnika: {ex.Message}");
            return View(journal);
        }
    }

    /// <summary>
    /// Forma za uređivanje aktivnosti - GET
    /// URL: /aktivnosti/uredi/{id}
    /// </summary>
    [Route("uredi/{id:int}")]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var activity = _activityRepository.GetById(id);
        if (activity == null)
            return NotFound();

        ViewBag.Users = _userRepository.GetAll();
        ViewBag.ActivityType = activity.ActivityType.ToString();
        
        return View(activity);
    }

    /// <summary>
    /// Spremi uređenu aktivnost - POST
    /// URL: /aktivnosti/uredi/{id}
    /// </summary>
    [Route("uredi/{id:int}")]
    [HttpPost]
    [ActionName("Edit")]
    public IActionResult EditPost(int id, Activity activity)
    {
        var existingActivity = _activityRepository.GetById(id);
        if (existingActivity == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Users = _userRepository.GetAll();
            ViewBag.ActivityType = existingActivity.ActivityType.ToString();
            return View(nameof(Edit), existingActivity);
        }

        try
        {
            // Ažuriraj samo dopuštena polja
            existingActivity.Title = activity.Title;
            existingActivity.Description = activity.Description;
            existingActivity.Difficulty = activity.Difficulty;
            existingActivity.CompletedDate = activity.CompletedDate;

            // Ažuriraj type-specifična polja
            if (existingActivity is Exercise exercise && activity is Exercise actExercise)
            {
                exercise.DurationMinutes = actExercise.DurationMinutes;
                exercise.CaloriesBurned = actExercise.CaloriesBurned;
                exercise.Sets = actExercise.Sets;
                exercise.Reps = actExercise.Reps;
                exercise.Weight = actExercise.Weight;
                exercise.Location = actExercise.Location;
                exercise.ExerciseType = actExercise.ExerciseType;
            }
            else if (existingActivity is Meditation meditation && activity is Meditation actMeditation)
            {
                meditation.DurationMinutes = actMeditation.DurationMinutes;
                meditation.MeditationType = actMeditation.MeditationType;
                meditation.FocusArea = actMeditation.FocusArea;
                meditation.StressReliefScore = actMeditation.StressReliefScore;
                meditation.MentalClarity = actMeditation.MentalClarity;
                meditation.Notes = actMeditation.Notes;
            }
            else if (existingActivity is DailyJournal journal && activity is DailyJournal actJournal)
            {
                journal.JournalDate = actJournal.JournalDate;
                journal.Reflection = actJournal.Reflection;
                journal.Mood = actJournal.Mood;
                journal.EnergyLevel = actJournal.EnergyLevel;
            }

            _activityRepository.Update(existingActivity);
            return RedirectToAction("Details", new { id = id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Greška pri ažuriranju aktivnosti: {ex.Message}");
            ViewBag.Users = _userRepository.GetAll();
            ViewBag.ActivityType = existingActivity.ActivityType.ToString();
            return View(nameof(Edit), existingActivity);
        }
    }

    /// <summary>
    /// Forma za brisanje aktivnosti - GET (potvrda)
    /// URL: /aktivnosti/obrisi/{id}
    /// </summary>
    [Route("obrisi/{id:int}")]
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var activity = _activityRepository.GetById(id);
        if (activity == null)
            return NotFound();

        return View(activity);
    }

    /// <summary>
    /// Briše aktivnost - POST
    /// URL: /aktivnosti/obrisi/{id}
    /// </summary>
    [Route("obrisi/{id:int}")]
    [HttpPost]
    [ActionName("Delete")]
    public IActionResult DeletePost(int id)
    {
        var activity = _activityRepository.GetById(id);
        if (activity == null)
            return NotFound();

        try
        {
            // Oduzmi XP od korisnika prije brisanja
            var user = _userRepository.GetById(activity.UserId);
            if (user != null && activity.XpReward > 0)
            {
                user.TotalXP = Math.Max(0, user.TotalXP - activity.XpReward);
                _userRepository.Update(user);
            }

            _activityRepository.Delete(id);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Greška pri brisanju aktivnosti: {ex.Message}");
            return View(nameof(Delete), activity);
        }
    }

    /// <summary>
    /// AJAX pretraga aktivnosti
    /// URL: /aktivnosti/pretraga?q=search_term
    /// </summary>
    [Route("pretraga")]
    [HttpGet]
    public IActionResult Search(string q, int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(new List<object>());

        var activities = userId.HasValue
            ? _activityRepository.GetByUserId(userId.Value)
            : _activityRepository.GetAll();

        var searchQuery = q.ToLower();
        var results = activities
            .Where(a => a.Title.ToLower().Contains(searchQuery) || 
                        a.Description.ToLower().Contains(searchQuery))
            .Take(10)
            .Select(a => new { 
                id = a.Id, 
                title = a.Title, 
                type = a.ActivityType.ToString(),
                user = a.User?.Username,
                completed = a.CompletedDate.ToString("dd.MM.yyyy")
            })
            .ToList();

        return Json(results);
    }
}
