using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;
using System.ComponentModel.DataAnnotations;
using Gamified_Self_Improvement.Services;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// API controller za autentikaciju i upravljanje korisnicima
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly GamefiedSelfImprovementDbContext _dbContext;
    private readonly UserSyncService _userSyncService;

    public AuthApiController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        GamefiedSelfImprovementDbContext dbContext,
        UserSyncService userSyncService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _userSyncService = userSyncService;
    }

    /// <summary>
    /// POST: /api/auth/register - Registracija novog korisnika
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Register([FromBody] CreateUserDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Provjeri postoji li korisnik s ovim emailom
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Korisnik sa ovim emailom već postoji" });
            }

            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                OIB = dto.OIB,
                JMBG = dto.JMBG,
                Bio = dto.Bio,
                PreferredMeditationType = dto.PreferredMeditationType,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Registracija nije uspjela", errors });
            }

            // Dodaj uloga User po defaultu
            await _userManager.AddToRoleAsync(user, "User");
            await _userSyncService.SyncFromAppUserAsync(user);

            // Prijavi korisnika
            await _signInManager.SignInAsync(user, isPersistent: true);

            var userDTO = MapUserToDTO(user);
            return Ok(new LoginResponseDTO
            {
                Success = true,
                Message = "Registracija uspješna",
                User = userDTO
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri registraciji", error = ex.Message });
        }
    }

    /// <summary>
    /// POST: /api/auth/login - Prijava korisnika
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user?.UserName == null)
            {
                return BadRequest(new { message = "Email ili lozinka nisu ispravni" });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, dto.Password, isPersistent: true, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Email ili lozinka nisu ispravni" });
            }

            var userDTO = MapUserToDTO(user);
            return Ok(new LoginResponseDTO
            {
                Success = true,
                Message = "Prijava uspješna",
                User = userDTO
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri prijavi", error = ex.Message });
        }
    }

    /// <summary>
    /// POST: /api/auth/logout - Odjava korisnika
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Odjava uspješna" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri odjavi", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/auth/me - Dohvati podatke trenutnog korisnika
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(MapUserToDTO(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju korisnika", error = ex.Message });
        }
    }

    /// <summary>
    /// GET: /api/auth/users/{id} - Dohvati korisnika po ID-u
    /// </summary>
    [HttpGet("users/{id}")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetUserById(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(MapUserToDTO(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri dohvaćanju korisnika", error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: /api/auth/profile - Ažuriraj korisnikove podatke
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> UpdateProfile([FromBody] UserDTO dto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Bio = dto.Bio;
            user.PreferredMeditationType = dto.PreferredMeditationType;
            user.FavoriteBooks = dto.FavoriteBooks;
            user.ProfileImagePath = dto.ProfileImagePath;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Ažuriranje nije uspjelo", errors });
            }

            return Ok(MapUserToDTO(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri ažuriranju profila", error = ex.Message });
        }
    }

    /// <summary>
    /// POST: /api/auth/change-password - Promjena lozinke
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Promjena lozinke nije uspjela", errors });
            }

            return Ok(new { message = "Lozinka je uspješno promijenjena" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Greška pri promjeni lozinke", error = ex.Message });
        }
    }

    private UserDTO MapUserToDTO(AppUser user)
    {
        return new UserDTO
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            TotalXP = user.TotalXP,
            Level = user.Level,
            Bio = user.Bio,
            ProfileImagePath = user.ProfileImagePath,
            CreatedDate = user.CreatedDate,
            PreferredMeditationType = user.PreferredMeditationType,
            FavoriteBooks = user.FavoriteBooks,
            StreakDays = user.StreakDays,
            LastActiveDate = user.LastActiveDate
        };
    }
}

/// <summary>
/// DTO za promjenu lozinke
/// </summary>
public class ChangePasswordDTO
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
