using Gamified_Self_Improvement.Repositories;
using GamefiedSelfImprovement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamified_Self_Improvement.Controllers;

/// <summary>
/// Controller za upravljanje aktivnostima - custom routing primjena
/// </summary>
[Route("aktivnosti")]
public class ActivityController : BaseController
{
    private readonly ActivityRepository _activityRepository;
    private readonly UserRepository _userRepository;
    private readonly GamefiedSelfImprovementDbContext _dbContext;
    private readonly IWebHostEnvironment _env;
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip" };

    public ActivityController(
        ActivityRepository activityRepository,
        UserRepository userRepository,
        UserManager<AppUser> userManager,
        GamefiedSelfImprovementDbContext dbContext,
        IWebHostEnvironment env)
        : base(userManager)
    {
        _activityRepository = activityRepository;
        _userRepository = userRepository;
        _dbContext = dbContext;
        _env = env;
    }

    /// <summary>
    /// Lista aktivnosti — admin/manager vide sve, korisnici samo svoje
    /// URL: /aktivnosti ili /aktivnosti/po-korisniku/{userId}
    /// </summary>
    [AllowAnonymous]
    [Route("")]
    [Route("po-korisniku/{userId:int}")]
    public async Task<IActionResult> Index(int? userId = null)
    {
        List<Activity> activities;

        if (userId.HasValue)
        {
            activities = _activityRepository.GetByUserId(userId.Value);
            ViewBag.UserId = userId;
            var legUser = _userRepository.GetById(userId.Value);
            ViewBag.UserName = legUser?.Username;
        }
        else if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            var legacyUser = await GetCurrentLegacyUserAsync();
            var appUserId = CurrentUserId;
            activities = _activityRepository.GetAll()
                .Where(a => a.AppUserId == appUserId || (legacyUser != null && a.UserId == legacyUser.Id))
                .ToList();
            ViewBag.UserName = legacyUser?.Username ?? User.Identity.Name;
        }
        else
        {
            activities = _activityRepository.GetAll();
        }

        return View(activities);
    }

    private async Task<User?> GetCurrentLegacyUserAsync()
    {
        var appUser = await UserManager.GetUserAsync(User);
        if (appUser == null) return null;
        return _userRepository.GetAll().FirstOrDefault(u => u.Email == appUser.Email);
    }

    private async Task UpdateUserProgressAsync(int xpEarned)
    {
        var appUser = await UserManager.GetUserAsync(User);
        if (appUser == null) return;

        appUser.TotalXP += xpEarned;
        appUser.Level = Math.Min(100, appUser.TotalXP / 100 + 1);
        appUser.LastActiveDate = DateTime.UtcNow;

        var streak = await _dbContext.Streaks.FirstOrDefaultAsync(s => s.UserId == appUser.Id);
        if (streak == null)
        {
            streak = new Streak(appUser.Id);
            _dbContext.Streaks.Add(streak);
        }
        streak.CheckAndResetStreak();
        streak.RecordActivity();
        appUser.StreakDays = streak.CurrentStreak;

        await UserManager.UpdateAsync(appUser);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SetCreateViewBagAsync()
    {
        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
        {
            ViewBag.Users = _userRepository.GetAll();
            ViewBag.IsOwnActivity = false;
        }
        else
        {
            var legacyUser = await GetCurrentLegacyUserAsync();
            ViewBag.Users = legacyUser != null ? new List<User> { legacyUser } : new List<User>();
            ViewBag.IsOwnActivity = true;
            ViewBag.CurrentLegacyUserId = legacyUser?.Id;
        }
    }

    private async Task AssignCurrentUserIfNeededAsync(Activity activity)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            var appUser = await UserManager.GetUserAsync(User);
            var legacyUser = await GetCurrentLegacyUserAsync();
            activity.AppUserId = appUser?.Id;
            activity.UserId = legacyUser?.Id;
            ModelState.Remove("UserId");
        }
    }

    /// <summary>
    /// Detalji o specifičnoj aktivnosti
    /// URL: /aktivnosti/{id}
    /// </summary>
    [Authorize]
    [Route("{id:int}")]
    public IActionResult Details(int id)
    {
        var activity = _activityRepository.GetById(id);
        if (activity == null)
            return NotFound();

        var user = activity.UserId.HasValue ? _userRepository.GetById(activity.UserId.Value) : null;
        ViewBag.UserName = user?.Username;

        return View(activity);
    }

    /// <summary>
    /// Forma za dodavanje nove vježbe - GET
    /// URL: /aktivnosti/nova-vjezba
    /// </summary>
    [Authorize]
    [Route("nova-vjezba")]
    [HttpGet]
    public async Task<IActionResult> CreateExercise()
    {
        await SetCreateViewBagAsync();
        return View();
    }

    /// <summary>
    /// Spremi novu vježbu - POST
    /// URL: /aktivnosti/nova-vjezba
    /// server side validacija
    /// </summary>
    [Authorize]
    [Route("nova-vjezba")]
    [HttpPost]
    public async Task<IActionResult> CreateExercise(Exercise exercise)
    {
        await AssignCurrentUserIfNeededAsync(exercise);

        if (!ModelState.IsValid)
        {
            await SetCreateViewBagAsync();
            return View(exercise);
        }

        try
        {
            exercise.ActivityType = ActivityType.Exercise;
            if (exercise.XpReward == 0)
                exercise.XpReward = exercise.CalculateXP();
            _activityRepository.Add(exercise);

            var user = exercise.UserId.HasValue ? _userRepository.GetById(exercise.UserId.Value) : null;
            if (user != null) { user.TotalXP += exercise.XpReward; _userRepository.Update(user); }

            await UpdateUserProgressAsync(exercise.XpReward);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await SetCreateViewBagAsync();
            ModelState.AddModelError("", $"Greška pri dodavanju vježbe: {ex.Message}");
            return View(exercise);
        }
    }

    /// <summary>
    /// Forma za dodavanje meditacije - GET
    /// URL: /aktivnosti/nova-meditacija
    /// </summary>
    [Authorize]
    [Route("nova-meditacija")]
    [HttpGet]
    public async Task<IActionResult> CreateMeditation()
    {
        await SetCreateViewBagAsync();
        return View();
    }

    /// <summary>
    /// Spremi novu meditaciju - POST
    /// URL: /aktivnosti/nova-meditacija
    /// </summary>
    [Authorize]
    [Route("nova-meditacija")]
    [HttpPost]
    public async Task<IActionResult> CreateMeditation(Meditation meditation)
    {
        await AssignCurrentUserIfNeededAsync(meditation);

        if (!ModelState.IsValid)
        {
            await SetCreateViewBagAsync();
            return View(meditation);
        }

        try
        {
            meditation.ActivityType = ActivityType.Meditation;
            if (meditation.XpReward == 0)
                meditation.XpReward = meditation.CalculateXP();
            _activityRepository.Add(meditation);

            var user = meditation.UserId.HasValue ? _userRepository.GetById(meditation.UserId.Value) : null;
            if (user != null) { user.TotalXP += meditation.XpReward; _userRepository.Update(user); }

            await UpdateUserProgressAsync(meditation.XpReward);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await SetCreateViewBagAsync();
            ModelState.AddModelError("", $"Greška pri dodavanju meditacije: {ex.Message}");
            return View(meditation);
        }
    }

    /// <summary>
    /// Forma za dodavanje unosa u dnevnik - GET
    /// URL: /aktivnosti/novi-dnevnik
    /// </summary>
    [Authorize]
    [Route("novi-dnevnik")]
    [HttpGet]
    public async Task<IActionResult> CreateJournal()
    {
        await SetCreateViewBagAsync();
        return View();
    }

    /// <summary>
    /// Spremi novi unos u dnevnik - POST
    /// URL: /aktivnosti/novi-dnevnik
    /// </summary>
    [Authorize]
    [Route("novi-dnevnik")]
    [HttpPost]
    public async Task<IActionResult> CreateJournal(DailyJournal journal)
    {
        await AssignCurrentUserIfNeededAsync(journal);

        if (!ModelState.IsValid)
        {
            await SetCreateViewBagAsync();
            return View(journal);
        }

        try
        {
            journal.ActivityType = ActivityType.Journal;
            if (journal.XpReward == 0)
                journal.XpReward = journal.CalculateXP();
            _activityRepository.Add(journal);

            var user = journal.UserId.HasValue ? _userRepository.GetById(journal.UserId.Value) : null;
            if (user != null) { user.TotalXP += journal.XpReward; _userRepository.Update(user); }

            await UpdateUserProgressAsync(journal.XpReward);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await SetCreateViewBagAsync();
            ModelState.AddModelError("", $"Greška pri dodavanju dnevnika: {ex.Message}");
            return View(journal);
        }
    }

    /// <summary>
    /// Forma za uređivanje aktivnosti - GET
    /// URL: /aktivnosti/uredi/{id}
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
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
    [Authorize(Roles = "Admin,Manager")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
            var user = activity.UserId.HasValue ? _userRepository.GetById(activity.UserId.Value) : null;
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
    [AllowAnonymous]
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

    /// <summary>
    /// AJAX popis priloga za aktivnost
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [Route("prilozi/{activityId:int}")]
    [HttpGet]
    public IActionResult GetAttachments(int activityId)
    {
        var attachments = _dbContext.Attachments
            .Where(a => a.ActivityId == activityId && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedDate)
            .ToList();

        return PartialView("_AttachmentList", attachments);
    }

    /// <summary>
    /// Asinkroni upload datoteke (Dropzone)
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [Route("upload-prilog")]
    [HttpPost]
    public async Task<IActionResult> UploadAttachment(int activityId, IFormFile file)
    {
        var activity = _activityRepository.GetById(activityId);
        if (activity == null)
        {
            return NotFound();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest();
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { message = "Datoteka je prevelika" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Tip datoteke nije dozvoljen" });
        }

        var uploadsPath = Path.Combine(
            _env.WebRootPath,
            "uploads",
            "activities",
            activityId.ToString());

        Directory.CreateDirectory(uploadsPath);

        var storedFileName = Guid.NewGuid().ToString() + extension;
        var physicalPath = Path.Combine(uploadsPath, storedFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Attachment
        {
            ActivityId = activityId,
            UserId = CurrentUserId,
            FileName = file.FileName,
            FilePath = $"/uploads/activities/{activityId}/{storedFileName}",
            ContentType = file.ContentType ?? "application/octet-stream",
            FileSize = file.Length,
            UploadedDate = DateTime.UtcNow
        };

        _dbContext.Attachments.Add(attachment);
        await _dbContext.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// Brisanje priloga (AJAX)
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [Route("obrisi-prilog")]
    [HttpPost]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await _dbContext.Attachments.FindAsync(id);
        if (attachment == null || attachment.IsDeleted)
        {
            return NotFound();
        }

        var physicalPath = Path.Combine(
            _env.WebRootPath,
            attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        _dbContext.Attachments.Remove(attachment);
        await _dbContext.SaveChangesAsync();

        return Json(new { success = true });
    }
}
