using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gamified_Self_Improvement.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpiritualBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookType = table.Column<int>(type: "int", nullable: false),
                    TotalPages = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chapters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiritualBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExerciseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Sets = table.Column<int>(type: "int", nullable: false),
                    Reps = table.Column<int>(type: "int", nullable: false),
                    RestSeconds = table.Column<int>(type: "int", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalXP = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredMeditationType = table.Column<int>(type: "int", nullable: false),
                    FavoriteBooks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreakDays = table.Column<int>(type: "int", nullable: false),
                    LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XPRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    XpAmount = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    UnlockedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XPRewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    XpReward = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    JournalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DailyGoals = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ambitions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accomplishments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reflection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mood = table.Column<int>(type: "int", nullable: true),
                    EnergyLevel = table.Column<int>(type: "int", nullable: true),
                    Challenges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId1 = table.Column<int>(type: "int", nullable: true),
                    ExerciseType = table.Column<int>(type: "int", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    CaloriesBurned = table.Column<int>(type: "int", nullable: true),
                    Sets = table.Column<int>(type: "int", nullable: true),
                    Reps = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MuscleGroups = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeditationType = table.Column<int>(type: "int", nullable: true),
                    Meditation_DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    AudioFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FocusArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StressReliefScore = table.Column<int>(type: "int", nullable: true),
                    MentalClarity = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookId = table.Column<int>(type: "int", nullable: true),
                    PagesRead = table.Column<int>(type: "int", nullable: true),
                    CurrentPage = table.Column<int>(type: "int", nullable: true),
                    SpiritualActivity_DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    SpiritualActivity_Reflection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_SpiritualBooks_BookId",
                        column: x => x.BookId,
                        principalTable: "SpiritualBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "SpiritualBooks",
                columns: new[] { "Id", "Author", "BookType", "Chapters", "Description", "IsAvailable", "Language", "Title", "TotalPages" },
                values: new object[,]
                {
                    { 1, "Razni autori", 0, "[]", "Kršćanski sveti tekst", true, "Croatian", "Biblija (Novi zavjet)", 260 },
                    { 2, "Prorok Muhammad (Mir budi s njim)", 1, "[]", "Islamski sveti tekst", true, "Croatian", "Kuran", 604 },
                    { 3, "Mojsije i drugi", 2, "[]", "Židovski sveti tekst", true, "Croatian", "Tora", 187 }
                });

            migrationBuilder.InsertData(
                table: "XPRewards",
                columns: new[] { "Id", "ActivityType", "Description", "Icon", "Name", "UnlockedDate", "XpAmount" },
                values: new object[,]
                {
                    { 1, 0, "", "💪", "Početni trening", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 50 },
                    { 2, 2, "", "🧘", "Dnevna meditacija", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 30 },
                    { 3, 1, "", "📖", "Čitanje duhovnog teksta", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 40 },
                    { 4, 3, "", "📝", "Refleksija u dnevniku", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 25 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_BookId",
                table: "Activities",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId",
                table: "Activities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId1",
                table: "Activities",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "TrainingLogs");

            migrationBuilder.DropTable(
                name: "XPRewards");

            migrationBuilder.DropTable(
                name: "SpiritualBooks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
