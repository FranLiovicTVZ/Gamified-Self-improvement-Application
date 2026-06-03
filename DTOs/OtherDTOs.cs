using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamefiedSelfImprovement.DTOs;

/// <summary>
/// DTO za duhovnu knjižu
/// </summary>
public class SpiritualBookDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public SpiritualBookType BookType { get; set; }
    public int TotalPages { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Language { get; set; } = "Croatian";
    public List<string> Chapters { get; set; } = new();
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// DTO za kreiranje/ažuriranje duhovne knjige
/// </summary>
public class CreateSpiritualBookDTO
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public SpiritualBookType BookType { get; set; }

    [Required]
    [Range(1, 10000)]
    public int TotalPages { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Author { get; set; } = string.Empty;

    public string Language { get; set; } = "Croatian";
    public List<string> Chapters { get; set; } = new();
}

/// <summary>
/// DTO za XP nagradu
/// </summary>
public class XPRewardDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int XpAmount { get; set; }
    public ActivityType ActivityType { get; set; }
    public DateTime UnlockedDate { get; set; }
    public string Icon { get; set; } = "🏆";
}

/// <summary>
/// DTO za trening log
/// </summary>
public class TrainingLogDTO
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int RestSeconds { get; set; } = 60;
    public DateTime LogDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
}

/// <summary>
/// DTO za kreiranje trening loga
/// </summary>
public class CreateTrainingLogDTO
{
    [Required]
    public int ExerciseId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string ExerciseName { get; set; } = string.Empty;

    [Required]
    [Range(0, 500)]
    public decimal Weight { get; set; }

    [Required]
    [Range(1, 100)]
    public int Sets { get; set; }

    [Required]
    [Range(1, 100)]
    public int Reps { get; set; }

    [Range(0, 300)]
    public int RestSeconds { get; set; } = 60;

    public string Notes { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
}

/// <summary>
/// DTO za priloženu datoteku
/// </summary>
public class AttachmentDTO
{
    public int Id { get; set; }
    public int? ActivityId { get; set; }
    public string? UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO za kreiranje priložene datoteke
/// </summary>
public class CreateAttachmentDTO
{
    public int? ActivityId { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO za streak - broj dana aktivnosti za redom
/// </summary>
public class StreakDTO
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? LastStreakResetDate { get; set; }
    public int TotalActivitiesCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

/// <summary>
/// DTO za update streaka
/// </summary>
public class UpdateStreakDTO
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
