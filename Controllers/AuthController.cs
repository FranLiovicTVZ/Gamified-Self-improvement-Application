using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Gamified_Self_Improvement.Services;
using GamefiedSelfImprovement.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamefiedSelfImprovement.Controllers;

/// <summary>
/// MVC controller za autentikaciju (prijava, registracija, OAuth)
/// </summary>
[Route("auth")]
public class AuthController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserSyncService _userSyncService;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        UserSyncService userSyncService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userSyncService = userSyncService;
        _configuration = configuration;
    }

    [Route("login")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? "/";
        SetOAuthViewBag();
        return View();
    }

    [Route("login")]
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? "/";
        SetOAuthViewBag();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email ili lozinka nisu ispravni.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, isPersistent: true, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Email ili lozinka nisu ispravni.");
            return View(model);
        }

        return await RedirectAfterLoginAsync(user, returnUrl);
    }

    [Route("register")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [Route("register")]
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "Korisnik s ovim emailom već postoji.");
            return View(model);
        }

        var user = new AppUser
        {
            UserName = model.UserName,
            Email = model.Email,
            OIB = model.OIB,
            JMBG = model.JMBG,
            EmailConfirmed = true,
            CreatedDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "User");
        await _userSyncService.SyncFromAppUserAsync(user);
        await _signInManager.SignInAsync(user, isPersistent: true);

        return await RedirectAfterLoginAsync(user, null);
    }

    [Route("logout")]
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Route("external-login")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        if (!IsProviderConfigured(provider))
        {
            TempData["AuthError"] = $"{provider} prijava nije konfigurirana. Postavi valjane kredencijale u appsettings.json ili user secrets.";
            return RedirectToAction(nameof(Login));
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [Route("external-login-callback")]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!string.IsNullOrEmpty(remoteError))
        {
            TempData["AuthError"] = $"Greška vanjskog providera: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["AuthError"] = "Vanjska prijava nije uspjela. Provjeri OAuth kredencijale i callback URL.";
            return RedirectToAction(nameof(Login));
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: true);

        if (signInResult.Succeeded)
        {
            var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existing != null)
            {
                await _userSyncService.SyncFromAppUserAsync(existing);
                return await RedirectAfterLoginAsync(existing, returnUrl);
            }
        }

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            TempData["AuthError"] = "Provider nije vratio email adresu.";
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ViewBag.Provider = info.LoginProvider;
            ViewBag.ReturnUrl = returnUrl;
            return View("ExternalLoginConfirm", new ExternalLoginConfirmViewModel
            {
                Email = email,
                UserName = info.Principal.Identity?.Name ?? email.Split('@')[0]
            });
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded && addLoginResult.Errors.Any(e => e.Code != "LoginAlreadyAssociated"))
        {
            TempData["AuthError"] = string.Join(" ", addLoginResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Login));
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
        await _userSyncService.SyncFromAppUserAsync(user);
        return await RedirectAfterLoginAsync(user, returnUrl);
    }

    [Route("external-login-confirm")]
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirm(ExternalLoginConfirmViewModel model, string? returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["AuthError"] = "Sesija vanjske prijave je istekla. Pokušaj ponovno.";
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var user = new AppUser
        {
            UserName = model.UserName,
            Email = model.Email,
            OIB = model.OIB,
            JMBG = model.JMBG,
            EmailConfirmed = true,
            CreatedDate = DateTime.UtcNow
        };

        var password = GenerateRandomPassword();
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "User");
        await _userManager.AddLoginAsync(user, info);
        await _userSyncService.SyncFromAppUserAsync(user);
        await _signInManager.SignInAsync(user, isPersistent: true);

        return await RedirectAfterLoginAsync(user, returnUrl);
    }

    private async Task<IActionResult> RedirectAfterLoginAsync(AppUser user, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl != "/" && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return RedirectToAction("AdminDashboard", "Home");
        }

        return RedirectToAction("UserDashboard", "Home");
    }

    private void SetOAuthViewBag()
    {
        var googleConfigured = IsProviderConfigured("Google");
        var facebookConfigured = IsProviderConfigured("Facebook");

        ViewBag.GoogleConfigured = googleConfigured;
        ViewBag.FacebookConfigured = facebookConfigured;
        ViewBag.OAuthConfigured = googleConfigured || facebookConfigured;
    }

    private bool IsProviderConfigured(string provider)
    {
        if (string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase))
        {
            return HasConfiguredValue("Authentication:Google:ClientId") &&
                   HasConfiguredValue("Authentication:Google:ClientSecret");
        }

        if (string.Equals(provider, "Facebook", StringComparison.OrdinalIgnoreCase))
        {
            return HasConfiguredValue("Authentication:Facebook:AppId") &&
                   HasConfiguredValue("Authentication:Facebook:AppSecret");
        }

        return false;
    }

    private bool HasConfiguredValue(string key)
    {
        var value = _configuration[key];
        return !string.IsNullOrWhiteSpace(value) &&
               !value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateRandomPassword()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes) + "Aa1!";
    }
}

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [Display(Name = "Korisničko ime")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
    [Display(Name = "OIB")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadržavati samo brojeve.")]
    [Display(Name = "JMBG")]
    public string JMBG { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Lozinka")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Potvrdi lozinku")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ExternalLoginConfirmViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$")]
    public string JMBG { get; set; } = string.Empty;
}
