using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamefiedSelfImprovement.DTOs;

/// <summary>
/// DTO za korisnika (AppUser)
/// </summary>
public class UserDTO
{
    public string? Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public int Level { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string ProfileImagePath { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public MeditationType PreferredMeditationType { get; set; }
    public List<int> FavoriteBooks { get; set; } = new();
    public int StreakDays { get; set; }
    public DateTime LastActiveDate { get; set; }
}

/// <summary>
/// DTO za kreiranje/ažuriranje korisnika
/// </summary>
public class CreateUserDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$")]
    public string JMBG { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;
    public MeditationType PreferredMeditationType { get; set; } = MeditationType.Mindfulness;
}

/// <summary>
/// DTO za login
/// </summary>
public class LoginDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO za vanjsku prijavu (OAuth)
/// </summary>
public class ExternalLoginDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? UserName { get; set; }

    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$")]
    public string OIB { get; set; } = string.Empty;

    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$")]
    public string JMBG { get; set; } = string.Empty;
}

/// <summary>
/// DTO za odgovor pri loginu
/// </summary>
public class LoginResponseDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDTO? User { get; set; }
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
