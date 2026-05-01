using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gamified_Self_Improvement.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Bio", "CreatedDate", "Email", "FavoriteBooks", "LastActiveDate", "Level", "PreferredMeditationType", "ProfileImagePath", "StreakDays", "TotalXP", "Username" },
                values: new object[,]
                {
                    { 1, "", new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ivan@example.com", "[]", new DateTime(2026, 5, 1, 19, 24, 39, 280, DateTimeKind.Local).AddTicks(8457), 5, 3, "", 0, 850, "ivan_temuhin" },
                    { 2, "", new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "marija@example.com", "[]", new DateTime(2026, 5, 1, 19, 24, 39, 281, DateTimeKind.Local).AddTicks(4230), 3, 1, "", 0, 420, "marija_fitness" },
                    { 3, "", new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "petar@example.com", "[]", new DateTime(2026, 5, 1, 19, 24, 39, 281, DateTimeKind.Local).AddTicks(4238), 7, 4, "", 0, 1200, "petar_spiritual" }
                });

            migrationBuilder.InsertData(
                table: "Activities",
                columns: new[] { "Id", "ActivityType", "CaloriesBurned", "CompletedDate", "Description", "Difficulty", "Discriminator", "DurationMinutes", "ExerciseType", "Location", "MuscleGroups", "Reps", "Sets", "Title", "UserId", "Weight", "XpReward" },
                values: new object[,]
                {
                    { 1, 0, 0, new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "5 km trčanja", 1, "Exercise", 30, 1, "Kuća", "[]", 0, 0, "Trčanje ujutro", 1, 0m, 0 },
                    { 2, 0, 0, new DateTime(2026, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Snaga - gornji dio tijela", 1, "Exercise", 45, 0, "Kuća", "[]", 10, 3, "Vježbanje u teretani", 1, 20m, 0 }
                });

            migrationBuilder.InsertData(
                table: "Activities",
                columns: new[] { "Id", "ActivityType", "AudioFilePath", "CompletedDate", "Description", "Difficulty", "Discriminator", "Meditation_DurationMinutes", "FocusArea", "MeditationType", "MentalClarity", "Notes", "StressReliefScore", "Title", "UserId", "XpReward" },
                values: new object[,]
                {
                    { 3, 2, "", new DateTime(2026, 5, 1, 19, 24, 39, 283, DateTimeKind.Local).AddTicks(9049), "10 minuta mindfulness meditacije", 1, "Meditation", 10, "Opća svjesnost", 3, 7, "", 8, "Ujutna meditacija", 2, 0 },
                    { 4, 2, "", new DateTime(2026, 5, 1, 19, 24, 39, 284, DateTimeKind.Local).AddTicks(1139), "15 minuta dišanja", 1, "Meditation", 15, "Opća svjesnost", 1, 8, "", 9, "Večernja meditacija", 2, 0 }
                });

            migrationBuilder.InsertData(
                table: "Activities",
                columns: new[] { "Id", "Accomplishments", "ActivityType", "Ambitions", "Challenges", "CompletedDate", "DailyGoals", "Description", "Difficulty", "Discriminator", "EnergyLevel", "JournalDate", "Mood", "Reflection", "Title", "UserId", "UserId1", "XpReward" },
                values: new object[] { 5, "[]", 3, "[]", "[]", new DateTime(2026, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "[]", "Razmišljanja o napretku", 1, "DailyJournal", 8, new DateTime(2026, 5, 1, 19, 24, 39, 284, DateTimeKind.Local).AddTicks(2439), 8, "", "Dnevna refleksija", 3, null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
