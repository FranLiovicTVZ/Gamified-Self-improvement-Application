using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamified_Self_Improvement.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Streaks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    LastActivityDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStreakResetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalActivitiesCompleted = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streaks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 3,
                column: "CompletedDate",
                value: new DateTime(2026, 6, 2, 12, 21, 21, 445, DateTimeKind.Local).AddTicks(7511));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 4,
                column: "CompletedDate",
                value: new DateTime(2026, 6, 2, 12, 21, 21, 445, DateTimeKind.Local).AddTicks(9699));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 5,
                column: "JournalDate",
                value: new DateTime(2026, 6, 2, 12, 21, 21, 446, DateTimeKind.Local).AddTicks(1059));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 2, 12, 21, 21, 443, DateTimeKind.Local).AddTicks(4218));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 2, 12, 21, 21, 443, DateTimeKind.Local).AddTicks(7678));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 2, 12, 21, 21, 443, DateTimeKind.Local).AddTicks(7684));

            migrationBuilder.CreateIndex(
                name: "IX_Streaks_UserId",
                table: "Streaks",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Streaks");

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 3,
                column: "CompletedDate",
                value: new DateTime(2026, 5, 26, 11, 17, 2, 348, DateTimeKind.Local).AddTicks(9134));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 4,
                column: "CompletedDate",
                value: new DateTime(2026, 5, 26, 11, 17, 2, 349, DateTimeKind.Local).AddTicks(1676));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 5,
                column: "JournalDate",
                value: new DateTime(2026, 5, 26, 11, 17, 2, 349, DateTimeKind.Local).AddTicks(3436));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastActiveDate",
                value: new DateTime(2026, 5, 26, 11, 17, 2, 345, DateTimeKind.Local).AddTicks(9258));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastActiveDate",
                value: new DateTime(2026, 5, 26, 11, 17, 2, 346, DateTimeKind.Local).AddTicks(2483));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastActiveDate",
                value: new DateTime(2026, 5, 26, 11, 17, 2, 346, DateTimeKind.Local).AddTicks(2489));
        }
    }
}
