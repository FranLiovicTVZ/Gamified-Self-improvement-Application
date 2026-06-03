using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace GamefiedSelfImprovement;

/// <summary>
/// DbContext klasa za Entity Framework - predstavlja bazu podataka
/// Nasljeđuje IdentityDbContext za Identity podrška
/// </summary>
public class GamefiedSelfImprovementDbContext : IdentityDbContext<AppUser>
{
    public GamefiedSelfImprovementDbContext(DbContextOptions<GamefiedSelfImprovementDbContext> options) 
        : base(options)
    {
    }

    // DbSet za sve entitete koji će biti tablice u bazi
    public new DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<SpiritualActivity> SpiritualActivities { get; set; }
    public DbSet<Meditation> Meditations { get; set; }
    public DbSet<DailyJournal> DailyJournals { get; set; }
    public DbSet<SpiritualBook> SpiritualBooks { get; set; }
    public DbSet<XPReward> XPRewards { get; set; }
    public DbSet<TrainingLog> TrainingLogs { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Streak> Streaks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Inicijalni korisnici
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "ivan_temuhin",
                Email = "ivan@example.com",
                CreatedDate = new DateTime(2026, 4, 1),
                TotalXP = 850,
                Level = 5,
                PreferredMeditationType = MeditationType.Mindfulness
            },
            new User 
            { 
                Id = 2, 
                Username = "marija_fitness",
                Email = "marija@example.com",
                CreatedDate = new DateTime(2026, 4, 15),
                TotalXP = 420,
                Level = 3,
                PreferredMeditationType = MeditationType.Breathing
            },
            new User 
            { 
                Id = 3, 
                Username = "petar_spiritual",
                Email = "petar@example.com",
                CreatedDate = new DateTime(2026, 3, 20),
                TotalXP = 1200,
                Level = 7,
                PreferredMeditationType = MeditationType.Visualization
            }
        );

        // Inicijalni duhovni tekstovi
        modelBuilder.Entity<SpiritualBook>().HasData(
            new SpiritualBook(1, "Biblija (Novi zavjet)", SpiritualBookType.Bible, 260)
            {
                Id = 1,
                Author = "Razni autori",
                Description = "Kršćanski sveti tekst",
                Language = "Croatian"
            },
            new SpiritualBook(2, "Kuran", SpiritualBookType.Quran, 604)
            {
                Id = 2,
                Author = "Prorok Muhammad (Mir budi s njim)",
                Description = "Islamski sveti tekst",
                Language = "Croatian"
            },
            new SpiritualBook(3, "Tora", SpiritualBookType.Torah, 187)
            {
                Id = 3,
                Author = "Mojsije i drugi",
                Description = "Židovski sveti tekst",
                Language = "Croatian"
            }
        );

        // Inicijalne nagrade
        modelBuilder.Entity<XPReward>().HasData(
            new { Id = 1, Name = "Početni trening", Description = "", XpAmount = 50, ActivityType = ActivityType.Exercise, UnlockedDate = new DateTime(2026, 4, 30), Icon = "💪" },
            new { Id = 2, Name = "Dnevna meditacija", Description = "", XpAmount = 30, ActivityType = ActivityType.Meditation, UnlockedDate = new DateTime(2026, 4, 30), Icon = "🧘" },
            new { Id = 3, Name = "Čitanje duhovnog teksta", Description = "", XpAmount = 40, ActivityType = ActivityType.Spiritual, UnlockedDate = new DateTime(2026, 4, 30), Icon = "📖" },
            new { Id = 4, Name = "Refleksija u dnevniku", Description = "", XpAmount = 25, ActivityType = ActivityType.Journal, UnlockedDate = new DateTime(2026, 4, 30), Icon = "📝" }
        );

        // Inicijalne aktivnosti
        modelBuilder.Entity<Exercise>().HasData(
            new Exercise 
            { 
                Id = 1, 
                UserId = 1,
                Title = "Trčanje ujutro",
                Description = "5 km trčanja",
                CompletedDate = new DateTime(2026, 4, 28),
                ActivityType = ActivityType.Exercise,
                DurationMinutes = 30,
                Sets = 0,
                Reps = 0,
                Weight = 0,
                ExerciseType = ExerciseType.Cardio
            },
            new Exercise 
            { 
                Id = 2, 
                UserId = 1,
                Title = "Vježbanje u teretani",
                Description = "Snaga - gornji dio tijela",
                CompletedDate = new DateTime(2026, 4, 29),
                ActivityType = ActivityType.Exercise,
                DurationMinutes = 45,
                Sets = 3,
                Reps = 10,
                Weight = 20,
                ExerciseType = ExerciseType.Strength
            }
        );

        modelBuilder.Entity<Meditation>().HasData(
            new Meditation 
            { 
                Id = 3, 
                UserId = 2,
                Title = "Ujutna meditacija",
                Description = "10 minuta mindfulness meditacije",
                CompletedDate = new DateTime(2026, 4, 28),
                ActivityType = ActivityType.Meditation,
                DurationMinutes = 10,
                MeditationType = MeditationType.Mindfulness,
                StressReliefScore = 8,
                MentalClarity = 7
            },
            new Meditation 
            { 
                Id = 4, 
                UserId = 2,
                Title = "Večernja meditacija",
                Description = "15 minuta dišanja",
                CompletedDate = new DateTime(2026, 4, 29),
                ActivityType = ActivityType.Meditation,
                DurationMinutes = 15,
                MeditationType = MeditationType.Breathing,
                StressReliefScore = 9,
                MentalClarity = 8
            }
        );

        modelBuilder.Entity<DailyJournal>().HasData(
            new DailyJournal 
            { 
                Id = 5, 
                UserId = 3,
                Title = "Dnevna refleksija",
                Description = "Razmišljanja o napretku",
                CompletedDate = new DateTime(2026, 4, 28),
                ActivityType = ActivityType.Journal,
                Mood = 8,
                EnergyLevel = 8
            }
        );
    }
}
