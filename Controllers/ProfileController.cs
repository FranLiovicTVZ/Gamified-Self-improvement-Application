using System.ComponentModel.DataAnnotations;
using Gamified_Self_Improvement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers;

/// <summary>
/// Controller za prikaz i uređivanje AppUser profila
/// </summary>
[Route("profil")]
[Authorize]
public class ProfileController : Controller
{
    private readonly GamefiedSelfImprovementDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly UserSyncService _userSyncService;

    public ProfileController(
        GamefiedSelfImprovementDbContext dbContext,
        UserManager<AppUser> userManager,
        UserSyncService userSyncService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _userSyncService = userSyncService;
    }

    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        await EnsureStreakAsync(user.Id);
        ViewBag.Streak = await GetStreakDtoAsync(user.Id);

        var activities = await _dbContext.Activities
            .Where(a => a.AppUserId == user.Id)
            .OrderByDescending(a => a.CompletedDate)
            .Take(10)
            .ToListAsync();

        ViewBag.Activities = activities;
        return View(user);
    }

    [Route("uredi")]
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        var model = new EditProfileViewModel
        {
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Bio = user.Bio,
            PreferredMeditationType = user.PreferredMeditationType
        };

        return View(model);
    }

    [Route("uredi")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.Bio = model.Bio;
        user.PreferredMeditationType = model.PreferredMeditationType;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _userSyncService.SyncFromAppUserAsync(user);
        TempData["ProfileMessage"] = "Profil je uspješno ažuriran.";
        return RedirectToAction(nameof(Index));
    }

    [Route("user/{userId}")]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ViewUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        await EnsureStreakAsync(user.Id);
        ViewBag.Streak = await GetStreakDtoAsync(user.Id);

        var activities = await _dbContext.Activities
            .Where(a => a.AppUserId == user.Id)
            .OrderByDescending(a => a.CompletedDate)
            .Take(10)
            .ToListAsync();

        ViewBag.Activities = activities;
        return View("Index", user);
    }

    private async Task EnsureStreakAsync(string userId)
    {
        var streak = await _dbContext.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        if (streak == null)
        {
            streak = new Streak(userId);
            _dbContext.Streaks.Add(streak);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<StreakDTO> GetStreakDtoAsync(string userId)
    {
        var streak = await _dbContext.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        if (streak == null)
        {
            return new StreakDTO { UserId = userId };
        }

        return new StreakDTO
        {
            Id = streak.Id,
            UserId = streak.UserId,
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            LastActivityDate = streak.LastActivityDate,
            LastStreakResetDate = streak.LastStreakResetDate,
            TotalActivitiesCompleted = streak.TotalActivitiesCompleted,
            CreatedDate = streak.CreatedDate,
            UpdatedDate = streak.UpdatedDate
        };
    }
}

public class EditProfileViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [Display(Name = "Korisničko ime")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "O meni")]
    public string Bio { get; set; } = string.Empty;

    [Display(Name = "Preferirani tip meditacije")]
    public MeditationType PreferredMeditationType { get; set; } = MeditationType.Mindfulness;
}
