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
    private readonly ILogger<ActivityController> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip" };

    public ActivityController(
        ActivityRepository activityRepository,
        UserRepository userRepository,
        UserManager<AppUser> userManager,
        GamefiedSelfImprovementDbContext dbContext,
        IWebHostEnvironment env,
        ILogger<ActivityController> logger)
        : base(userManager)
    {
        _activityRepository = activityRepository;
        _userRepository = userRepository;
        _dbContext = dbContext;
        _env = env;
        _logger = logger;
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

    private async Task UpdateUserProgressAsync(int xpEarned, ActivityType activityType)
    {
        var appUser = await UserManager.GetUserAsync(User);
        if (appUser == null) return;

        appUser.TotalXP += xpEarned;
        appUser.Level = Math.Min(100, appUser.TotalXP / 100 + 1);
        appUser.LastActiveDate = DateTime.UtcNow;

        switch (activityType)
        {
            case ActivityType.Exercise:   appUser.ExerciseXP  += xpEarned; break;
            case ActivityType.Meditation: appUser.MeditationXP += xpEarned; break;
            case ActivityType.Journal:    appUser.JournalXP   += xpEarned; break;
        }

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
        else if (activity.UserId == null || activity.UserId == 0)
        {
            // Admin/Manager koji nije odabrao korisnika — pripiši samome sebi
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
        ViewBag.IsOwner = activity.AppUserId == CurrentUserId;

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
        return View(new Exercise());
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

        exercise.Description ??= string.Empty;
        exercise.Location ??= "Kuća";

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

            await UpdateUserProgressAsync(exercise.XpReward, ActivityType.Exercise);
            _logger.LogInformation("Korisnik {UserId} kreirao vježbu '{Title}' (XP: {Xp})", CurrentUserId, exercise.Title, exercise.XpReward);
            return RedirectToAction("Edit", new { id = exercise.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri kreiranju vježbe za korisnika {UserId}", CurrentUserId);
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
        return View(new Meditation());
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

        // ASP.NET Core 8 s nullable annotations konvertira prazne stringove u null;
        // normaliziramo ih na prazne stringove kako bismo zadovoljili DB NOT NULL constraint.
        meditation.Description ??= string.Empty;
        meditation.AudioFilePath ??= string.Empty;
        meditation.FocusArea ??= "Opća svjesnost";
        meditation.Notes ??= string.Empty;

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

            await UpdateUserProgressAsync(meditation.XpReward, ActivityType.Meditation);
            _logger.LogInformation("Korisnik {UserId} kreirao meditaciju '{Title}' (XP: {Xp})", CurrentUserId, meditation.Title, meditation.XpReward);
            return RedirectToAction("Edit", new { id = meditation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri kreiranju meditacije za korisnika {UserId}", CurrentUserId);
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
        return View(new DailyJournal());
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

        // Dnevnik nema polje DurationMinutes u formi — ukloni validacijsku grešku
        ModelState.Remove("DurationMinutes");

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

            await UpdateUserProgressAsync(journal.XpReward, ActivityType.Journal);
            _logger.LogInformation("Korisnik {UserId} kreirao dnevnički unos '{Title}' (XP: {Xp})", CurrentUserId, journal.Title, journal.XpReward);
            return RedirectToAction("Edit", new { id = journal.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri kreiranju dnevnika za korisnika {UserId}", CurrentUserId);
            await SetCreateViewBagAsync();
            ModelState.AddModelError("", $"Greška pri dodavanju dnevnika: {ex.Message}");
            return View(journal);
        }
    }

    private bool CanEditActivity(Activity activity) =>
        activity.AppUserId == CurrentUserId ||
        User.IsInRole("Admin") ||
        User.IsInRole("Manager");

    /// <summary>
    /// Forma za uređivanje aktivnosti - GET
    /// URL: /aktivnosti/uredi/{id}
    /// </summary>
    [Authorize]
    [Route("uredi/{id:int}")]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var activity = _activityRepository.GetById(id);
        if (activity == null)
            return NotFound();

        if (!CanEditActivity(activity))
            return Forbid();

        ViewBag.Users = User.IsInRole("Admin") || User.IsInRole("Manager")
            ? _userRepository.GetAll()
            : (IEnumerable<User>)new[] { _userRepository.GetAll().FirstOrDefault(u => u.Id == activity.UserId)! }.Where(u => u != null);
        ViewBag.ActivityType = activity.ActivityType.ToString();

        return View(activity);
    }

    /// <summary>
    /// Spremi uređenu aktivnost - POST
    /// URL: /aktivnosti/uredi/{id}
    /// </summary>
    [Authorize]
    [Route("uredi/{id:int}")]
    [HttpPost]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id)
    {
        var existingActivity = _activityRepository.GetById(id);
        if (existingActivity == null)
            return NotFound();

        if (!CanEditActivity(existingActivity))
            return Forbid();

        if (!await TryUpdateModelAsync(existingActivity))
        {
            ViewBag.Users = User.IsInRole("Admin") || User.IsInRole("Manager")
                ? _userRepository.GetAll()
                : (IEnumerable<User>)new[] { _userRepository.GetAll().FirstOrDefault(u => u.Id == existingActivity.UserId)! }.Where(u => u != null);
            ViewBag.ActivityType = existingActivity.ActivityType.ToString();
            return View(nameof(Edit), existingActivity);
        }

        try
        {
            _activityRepository.Update(existingActivity);
            _logger.LogInformation("Korisnik {UserId} ažurirao aktivnost {ActivityId}", CurrentUserId, id);
            return RedirectToAction("Details", new { id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri ažuriranju aktivnosti {ActivityId} za korisnika {UserId}", id, CurrentUserId);
            ModelState.AddModelError("", $"Greška pri ažuriranju aktivnosti: {ex.Message}");
            ViewBag.Users = User.IsInRole("Admin") || User.IsInRole("Manager")
                ? _userRepository.GetAll()
                : (IEnumerable<User>)new[] { _userRepository.GetAll().FirstOrDefault(u => u.Id == existingActivity.UserId)! }.Where(u => u != null);
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
            _logger.LogWarning("Korisnik {UserId} obrisao aktivnost {ActivityId} '{Title}'", CurrentUserId, id, activity.Title);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri brisanju aktivnosti {ActivityId}", id);
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
    [Authorize]
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
    [Authorize]
    [Route("upload-prilog")]
    [HttpPost]
    public async Task<IActionResult> UploadAttachment(int activityId, IFormFile file)
    {
        var activity = _activityRepository.GetById(activityId);
        if (activity == null)
            return NotFound();

        if (!CanEditActivity(activity))
            return Forbid();

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

        _logger.LogInformation("Korisnik {UserId} uploadao prilog '{FileName}' za aktivnost {ActivityId}", CurrentUserId, file.FileName, activityId);
        return Json(new { success = true });
    }

    /// <summary>
    /// Brisanje priloga (AJAX)
    /// </summary>
    [Authorize]
    [Route("obrisi-prilog")]
    [HttpPost]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await _dbContext.Attachments.FindAsync(id);
        if (attachment == null || attachment.IsDeleted)
            return NotFound();

        // Provjeri vlasništvo (korisnik može brisati samo vlastite priloge)
        if (attachment.UserId != CurrentUserId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            return Forbid();

        var physicalPath = Path.Combine(
            _env.WebRootPath,
            attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        _dbContext.Attachments.Remove(attachment);
        await _dbContext.SaveChangesAsync();

        _logger.LogWarning("Korisnik {UserId} obrisao prilog {AttachmentId} '{FileName}'", CurrentUserId, id, attachment.FileName);
        return Json(new { success = true });
    }
}
