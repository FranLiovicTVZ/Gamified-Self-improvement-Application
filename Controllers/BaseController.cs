using GamefiedSelfImprovement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gamified_Self_Improvement.Controllers;

/// <summary>
/// Bazni MVC controller s pristupom trenutnom korisniku
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly UserManager<AppUser> UserManager;

    protected BaseController(UserManager<AppUser> userManager)
    {
        UserManager = userManager;
    }

    protected string? CurrentUserId => UserManager.GetUserId(User);
}
