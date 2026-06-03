using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamefiedSelfImprovement.DTOs;

/// <summary>
/// Bazni DTO za sve aktivnosti
/// </summary>
public class ActivityDTO
{
    public int Id { get; set; }
    public string? AppUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CompletedDate { get; set; }
    public int XpReward { get; set; }
    public ActivityType ActivityType { get; set; }
    public DifficultyLevel Difficulty { get; set; }
}

/// <summary>
/// DTO za vježbu (Exercise)
/// </summary>
public class ExerciseDTO : ActivityDTO
{
    public ExerciseType ExerciseType { get; set; }
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal Weight { get; set; }
    public List<string> MuscleGroups { get; set; } = new();
    public string Location { get; set; } = "Kuća";
}

/// <summary>
/// DTO za kreiranje/ažuriranje vježbe
/// </summary>
public class CreateExerciseDTO
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ExerciseType ExerciseType { get; set; }

    [Required]
    [Range(1, 480)]
    public int DurationMinutes { get; set; }

    [Range(0, 5000)]
    public int CaloriesBurned { get; set; }

    [Range(0, 100)]
    public int Sets { get; set; }

    [Range(0, 100)]
    public int Reps { get; set; }

    [Range(0, 500)]
    public decimal Weight { get; set; }

    public List<string> MuscleGroups { get; set; } = new();
    public string Location { get; set; } = "Kuća";
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
}

/// <summary>
/// DTO za kreiranje/ažuriranje duhovne aktivnosti
/// </summary>
public class CreateSpiritualActivityDTO
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int BookId { get; set; }

    [Range(0, 10000)]
    public int PagesRead { get; set; }

    [Range(0, 10000)]
    public int CurrentPage { get; set; }

    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    public string Reflection { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
}

/// <summary>
/// DTO za duhovnu aktivnost
/// </summary>
public class SpiritualActivityDTO : ActivityDTO
{
    public int BookId { get; set; }
    public SpiritualBookDTO? Book { get; set; }
    public int PagesRead { get; set; }
    public int CurrentPage { get; set; }
    public int DurationMinutes { get; set; }
    public string Reflection { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime StartDate { get; set; }
}

/// <summary>
/// DTO za meditaciju
/// </summary>
public class MeditationDTO : ActivityDTO
{
    public MeditationType MeditationType { get; set; }
    public int DurationMinutes { get; set; }
    public string AudioFilePath { get; set; } = string.Empty;
    public string FocusArea { get; set; } = "Opća svjesnost";
    public int StressReliefScore { get; set; }
    public int MentalClarity { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// DTO za dnevnik
/// </summary>
public class DailyJournalDTO : ActivityDTO
{
    public DateTime JournalDate { get; set; }
    public List<string> DailyGoals { get; set; } = new();
    public List<string> Ambitions { get; set; } = new();
    public List<string> Accomplishments { get; set; } = new();
    public string Reflection { get; set; } = string.Empty;
    public int Mood { get; set; }
    public int EnergyLevel { get; set; }
    public List<string> Challenges { get; set; } = new();
}

/// <summary>
/// DTO za kreiranje meditacije
/// </summary>
public class CreateMeditationDTO
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public MeditationType MeditationType { get; set; }

    [Required]
    [Range(1, 300)]
    public int DurationMinutes { get; set; }

    public string AudioFilePath { get; set; } = string.Empty;
    public string FocusArea { get; set; } = "Opća svjesnost";

    [Range(1, 10)]
    public int StressReliefScore { get; set; }

    [Range(1, 10)]
    public int MentalClarity { get; set; }

    public string Notes { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
}
