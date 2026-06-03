using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamefiedSelfImprovement.DTOs;

namespace GamefiedSelfImprovement.Controllers.Api;

/// <summary>
/// Bazni API controller sa zajedničkim metodama
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly GamefiedSelfImprovementDbContext _dbContext;

    public BaseApiController(GamefiedSelfImprovementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Mapiraj Exercise u ExerciseDTO
    /// </summary>
    protected ExerciseDTO MapExerciseToDTO(Exercise exercise)
    {
        return new ExerciseDTO
        {
            Id = exercise.Id,
            AppUserId = exercise.AppUserId,
            Title = exercise.Title,
            Description = exercise.Description,
            CompletedDate = exercise.CompletedDate,
            XpReward = exercise.XpReward,
            ActivityType = exercise.ActivityType,
            Difficulty = exercise.Difficulty,
            ExerciseType = exercise.ExerciseType,
            DurationMinutes = exercise.DurationMinutes,
            CaloriesBurned = exercise.CaloriesBurned,
            Sets = exercise.Sets,
            Reps = exercise.Reps,
            Weight = exercise.Weight,
            MuscleGroups = exercise.MuscleGroups,
            Location = exercise.Location
        };
    }

    /// <summary>
    /// Mapiraj Meditation u MeditationDTO
    /// </summary>
    protected MeditationDTO MapMeditationToDTO(Meditation meditation)
    {
        return new MeditationDTO
        {
            Id = meditation.Id,
            AppUserId = meditation.AppUserId,
            Title = meditation.Title,
            Description = meditation.Description,
            CompletedDate = meditation.CompletedDate,
            XpReward = meditation.XpReward,
            ActivityType = meditation.ActivityType,
            Difficulty = meditation.Difficulty,
            MeditationType = meditation.MeditationType,
            DurationMinutes = meditation.DurationMinutes,
            AudioFilePath = meditation.AudioFilePath,
            FocusArea = meditation.FocusArea,
            StressReliefScore = meditation.StressReliefScore,
            MentalClarity = meditation.MentalClarity,
            Notes = meditation.Notes
        };
    }

    /// <summary>
    /// Mapiraj DailyJournal u DailyJournalDTO
    /// </summary>
    protected DailyJournalDTO MapDailyJournalToDTO(DailyJournal journal)
    {
        return new DailyJournalDTO
        {
            Id = journal.Id,
            AppUserId = journal.AppUserId,
            Title = journal.Title,
            Description = journal.Description,
            CompletedDate = journal.CompletedDate,
            XpReward = journal.XpReward,
            ActivityType = journal.ActivityType,
            Difficulty = journal.Difficulty,
            JournalDate = journal.JournalDate,
            DailyGoals = journal.DailyGoals,
            Ambitions = journal.Ambitions,
            Accomplishments = journal.Accomplishments,
            Reflection = journal.Reflection,
            Mood = journal.Mood,
            EnergyLevel = journal.EnergyLevel,
            Challenges = journal.Challenges
        };
    }

    /// <summary>
    /// Mapiraj SpiritualActivity u SpiritualActivityDTO
    /// </summary>
    protected SpiritualActivityDTO MapSpiritualActivityToDTO(SpiritualActivity activity)
    {
        return new SpiritualActivityDTO
        {
            Id = activity.Id,
            AppUserId = activity.AppUserId,
            Title = activity.Title,
            Description = activity.Description,
            CompletedDate = activity.CompletedDate,
            XpReward = activity.XpReward,
            ActivityType = activity.ActivityType,
            Difficulty = activity.Difficulty,
            BookId = activity.BookId,
            Book = activity.Book != null ? MapSpiritualBookToDTO(activity.Book) : null,
            PagesRead = activity.PagesRead,
            CurrentPage = activity.CurrentPage,
            DurationMinutes = activity.DurationMinutes,
            Reflection = activity.Reflection,
            IsCompleted = activity.IsCompleted,
            StartDate = activity.StartDate
        };
    }

    /// <summary>
    /// Mapiraj SpiritualBook u SpiritualBookDTO
    /// </summary>
    protected SpiritualBookDTO MapSpiritualBookToDTO(SpiritualBook book)
    {
        return new SpiritualBookDTO
        {
            Id = book.Id,
            Title = book.Title,
            BookType = book.BookType,
            TotalPages = book.TotalPages,
            Description = book.Description,
            Author = book.Author,
            Language = book.Language,
            Chapters = book.Chapters,
            IsAvailable = book.IsAvailable
        };
    }

    /// <summary>
    /// Mapiraj XPReward u XPRewardDTO
    /// </summary>
    protected XPRewardDTO MapXPRewardToDTO(XPReward reward)
    {
        return new XPRewardDTO
        {
            Id = reward.Id,
            Name = reward.Name,
            Description = reward.Description,
            XpAmount = reward.XpAmount,
            ActivityType = reward.ActivityType,
            UnlockedDate = reward.UnlockedDate,
            Icon = reward.Icon
        };
    }

    /// <summary>
    /// Mapiraj TrainingLog u TrainingLogDTO
    /// </summary>
    protected TrainingLogDTO MapTrainingLogToDTO(TrainingLog log)
    {
        return new TrainingLogDTO
        {
            Id = log.Id,
            ExerciseId = log.ExerciseId,
            UserId = log.UserId,
            ExerciseName = log.ExerciseName,
            Weight = log.Weight,
            Sets = log.Sets,
            Reps = log.Reps,
            RestSeconds = log.RestSeconds,
            LogDate = log.LogDate,
            Notes = log.Notes,
            Difficulty = log.Difficulty
        };
    }

    /// <summary>
    /// Mapiraj Attachment u AttachmentDTO
    /// </summary>
    protected AttachmentDTO MapAttachmentToDTO(Attachment attachment)
    {
        return new AttachmentDTO
        {
            Id = attachment.Id,
            ActivityId = attachment.ActivityId,
            UserId = attachment.UserId,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedDate = attachment.UploadedDate,
            Description = attachment.Description
        };
    }

    /// <summary>
    /// Mapiraj AppUser u UserDTO
    /// </summary>
    protected UserDTO MapUserToDTO(AppUser user)
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

    /// <summary>
    /// Mapiraj Streak u StreakDTO
    /// </summary>
    protected StreakDTO MapStreakToDTO(Streak streak)
    {
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

    /// <summary>
    /// Ažuriraj streak korisnika kada je obavio aktivnost
    /// </summary>
    protected async Task<Streak> UpdateStreakForUserAsync(string userId)
    {
        var streak = await _dbContext.Streaks
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (streak == null)
        {
            // Kreiraj novi streak ako ne postoji
            streak = new Streak(userId);
            _dbContext.Streaks.Add(streak);
            await _dbContext.SaveChangesAsync();
        }

        // Provjeri da li trebam resetirati streak
        streak.CheckAndResetStreak();

        // Zapiši novu aktivnost
        streak.RecordActivity();
        _dbContext.Streaks.Update(streak);
        await _dbContext.SaveChangesAsync();

        return streak;
    }
}
