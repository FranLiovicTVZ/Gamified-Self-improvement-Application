using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace GamefiedSelfImprovement.McpTools;

[McpServerToolType]
public sealed class ActivityTools
{
    [McpServerTool, Description("List exercises with optional filters. Returns up to 20 most recent.")]
    public static async Task<string> ListExercises(
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity Id to filter by (optional)")] string? userId = null,
        [Description("Exercise type filter: Strength, Cardio, Flexibility, Sports")] string? type = null,
        [Description("Difficulty filter: Easy, Medium, Hard, Extreme")] string? difficulty = null)
    {
        var query = db.Exercises.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(e => e.AppUserId == userId);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<ExerciseType>(type, true, out var t))
            query = query.Where(e => e.ExerciseType == t);

        if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var d))
            query = query.Where(e => e.Difficulty == d);

        var items = await query.OrderByDescending(e => e.CompletedDate).Take(20).ToListAsync();

        return JsonSerializer.Serialize(items.Select(e => new
        {
            e.Id,
            e.AppUserId,
            e.Title,
            ExerciseType = e.ExerciseType.ToString(),
            e.DurationMinutes,
            e.CaloriesBurned,
            e.Sets,
            e.Reps,
            e.XpReward,
            Difficulty = e.Difficulty.ToString(),
            e.CompletedDate
        }));
    }

    [McpServerTool, Description("Log a new exercise activity. Calculates XP and updates user streak automatically.")]
    public static async Task<string> LogExercise(
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity string Id (get from list_users)")] string appUserId,
        [Description("Title of the exercise")] string title,
        [Description("Type: Strength, Cardio, Flexibility, or Sports")] string exerciseType,
        [Description("Duration in minutes (1-480)")] int durationMinutes,
        [Description("Difficulty: Easy, Medium, Hard, or Extreme")] string difficulty,
        [Description("Optional description")] string? description = null,
        [Description("Calories burned")] int calories = 0,
        [Description("Number of sets")] int sets = 0,
        [Description("Reps per set")] int reps = 0)
    {
        if (!Enum.TryParse<ExerciseType>(exerciseType, true, out var exType))
            return JsonSerializer.Serialize(new { error = $"Invalid exerciseType '{exerciseType}'. Valid values: Strength, Cardio, Flexibility, Sports" });

        if (!Enum.TryParse<DifficultyLevel>(difficulty, true, out var diff))
            return JsonSerializer.Serialize(new { error = $"Invalid difficulty '{difficulty}'. Valid values: Easy, Medium, Hard, Extreme" });

        var exercise = new Exercise
        {
            AppUserId = appUserId,
            Title = title,
            Description = description ?? string.Empty,
            ExerciseType = exType,
            DurationMinutes = durationMinutes,
            CaloriesBurned = calories,
            Sets = sets,
            Reps = reps,
            Difficulty = diff,
            ActivityType = ActivityType.Exercise,
            CompletedDate = DateTime.UtcNow
        };
        exercise.XpReward = exercise.CalculateXP();

        db.Exercises.Add(exercise);
        await db.SaveChangesAsync();
        await UpdateStreakAsync(db, appUserId);

        return JsonSerializer.Serialize(new
        {
            success = true,
            exercise.Id,
            exercise.Title,
            xpEarned = exercise.XpReward,
            message = $"Exercise logged! Earned {exercise.XpReward} XP."
        });
    }

    [McpServerTool, Description("List meditations with optional filters. Returns up to 20 most recent.")]
    public static async Task<string> ListMeditations(
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity Id to filter by (optional)")] string? userId = null,
        [Description("Type filter: Guided, Breathing, Mantras, Mindfulness, Visualization")] string? type = null,
        [Description("Difficulty filter: Easy, Medium, Hard, Extreme")] string? difficulty = null)
    {
        var query = db.Meditations.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(m => m.AppUserId == userId);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<MeditationType>(type, true, out var t))
            query = query.Where(m => m.MeditationType == t);

        if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var d))
            query = query.Where(m => m.Difficulty == d);

        var items = await query.OrderByDescending(m => m.CompletedDate).Take(20).ToListAsync();

        return JsonSerializer.Serialize(items.Select(m => new
        {
            m.Id,
            m.AppUserId,
            m.Title,
            MeditationType = m.MeditationType.ToString(),
            m.DurationMinutes,
            m.StressReliefScore,
            m.MentalClarity,
            m.XpReward,
            Difficulty = m.Difficulty.ToString(),
            m.CompletedDate
        }));
    }

    [McpServerTool, Description("Log a new meditation session. Calculates XP and updates user streak automatically.")]
    public static async Task<string> LogMeditation(
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity string Id (get from list_users)")] string appUserId,
        [Description("Title of the meditation")] string title,
        [Description("Type: Guided, Breathing, Mantras, Mindfulness, or Visualization")] string meditationType,
        [Description("Duration in minutes (1-300)")] int durationMinutes,
        [Description("Difficulty: Easy, Medium, Hard, or Extreme")] string difficulty,
        [Description("Stress relief score 1-10")] int stressReliefScore = 5,
        [Description("Mental clarity score 1-10")] int mentalClarity = 5,
        [Description("Optional notes")] string? notes = null)
    {
        if (!Enum.TryParse<MeditationType>(meditationType, true, out var medType))
            return JsonSerializer.Serialize(new { error = $"Invalid meditationType '{meditationType}'. Valid values: Guided, Breathing, Mantras, Mindfulness, Visualization" });

        if (!Enum.TryParse<DifficultyLevel>(difficulty, true, out var diff))
            return JsonSerializer.Serialize(new { error = $"Invalid difficulty '{difficulty}'. Valid values: Easy, Medium, Hard, Extreme" });

        var meditation = new Meditation
        {
            AppUserId = appUserId,
            Title = title,
            DurationMinutes = durationMinutes,
            MeditationType = medType,
            Difficulty = diff,
            StressReliefScore = Math.Clamp(stressReliefScore, 1, 10),
            MentalClarity = Math.Clamp(mentalClarity, 1, 10),
            Notes = notes ?? string.Empty,
            ActivityType = ActivityType.Meditation,
            CompletedDate = DateTime.UtcNow
        };
        meditation.XpReward = meditation.CalculateXP();

        db.Meditations.Add(meditation);
        await db.SaveChangesAsync();
        await UpdateStreakAsync(db, appUserId);

        return JsonSerializer.Serialize(new
        {
            success = true,
            meditation.Id,
            meditation.Title,
            xpEarned = meditation.XpReward,
            message = $"Meditation logged! Earned {meditation.XpReward} XP."
        });
    }

    [McpServerTool, Description("Log a daily journal entry. Calculates XP and updates user streak automatically.")]
    public static async Task<string> LogJournal(
        GamefiedSelfImprovementDbContext db,
        [Description("AppUser Identity string Id (get from list_users)")] string appUserId,
        [Description("Journal title")] string title,
        [Description("Mood score 1-10")] int mood,
        [Description("Energy level score 1-10")] int energyLevel,
        [Description("Reflection text")] string? reflection = null,
        [Description("Comma-separated list of daily goals")] string? goals = null,
        [Description("Comma-separated list of accomplishments")] string? accomplishments = null)
    {
        var journal = new DailyJournal
        {
            AppUserId = appUserId,
            Title = title,
            Mood = Math.Clamp(mood, 1, 10),
            EnergyLevel = Math.Clamp(energyLevel, 1, 10),
            Reflection = reflection ?? string.Empty,
            DailyGoals = ParseCsv(goals),
            Accomplishments = ParseCsv(accomplishments),
            JournalDate = DateTime.UtcNow,
            ActivityType = ActivityType.Journal,
            CompletedDate = DateTime.UtcNow
        };
        journal.XpReward = journal.CalculateXP();

        db.DailyJournals.Add(journal);
        await db.SaveChangesAsync();
        await UpdateStreakAsync(db, appUserId);

        return JsonSerializer.Serialize(new
        {
            success = true,
            journal.Id,
            journal.Title,
            xpEarned = journal.XpReward,
            message = $"Journal logged! Earned {journal.XpReward} XP."
        });
    }

    private static List<string> ParseCsv(string? csv) =>
        string.IsNullOrWhiteSpace(csv) ? [] :
        [.. csv.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0)];

    private static async Task UpdateStreakAsync(GamefiedSelfImprovementDbContext db, string userId)
    {
        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        if (streak == null)
        {
            streak = new Streak(userId);
            db.Streaks.Add(streak);
            await db.SaveChangesAsync();
        }
        streak.CheckAndResetStreak();
        streak.RecordActivity();
        db.Streaks.Update(streak);
        await db.SaveChangesAsync();
    }
}
