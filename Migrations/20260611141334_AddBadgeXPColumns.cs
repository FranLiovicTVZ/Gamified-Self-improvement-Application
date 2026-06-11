using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamified_Self_Improvement.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgeXPColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExerciseXP",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JournalXP",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MeditationXP",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 3,
                column: "CompletedDate",
                value: new DateTime(2026, 6, 11, 16, 13, 32, 836, DateTimeKind.Local).AddTicks(6917));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 4,
                column: "CompletedDate",
                value: new DateTime(2026, 6, 11, 16, 13, 32, 836, DateTimeKind.Local).AddTicks(8873));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 5,
                column: "JournalDate",
                value: new DateTime(2026, 6, 11, 16, 13, 32, 837, DateTimeKind.Local).AddTicks(95));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 11, 16, 13, 32, 833, DateTimeKind.Local).AddTicks(6526));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 11, 16, 13, 32, 834, DateTimeKind.Local).AddTicks(2723));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 11, 16, 13, 32, 834, DateTimeKind.Local).AddTicks(2730));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseXP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "JournalXP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MeditationXP",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 3,
                column: "CompletedDate",
                value: new DateTime(2026, 6, 11, 16, 3, 30, 314, DateTimeKind.Local).AddTicks(6857));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 4,
                column: "CompletedDate",
                value: new DateTime(2026, 6, 11, 16, 3, 30, 314, DateTimeKind.Local).AddTicks(9196));

            migrationBuilder.UpdateData(
                table: "Activities",
                keyColumn: "Id",
                keyValue: 5,
                column: "JournalDate",
                value: new DateTime(2026, 6, 11, 16, 3, 30, 315, DateTimeKind.Local).AddTicks(554));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 11, 16, 3, 30, 311, DateTimeKind.Local).AddTicks(2741));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 11, 16, 3, 30, 312, DateTimeKind.Local).AddTicks(790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastActiveDate",
                value: new DateTime(2026, 6, 11, 16, 3, 30, 312, DateTimeKind.Local).AddTicks(817));
        }
    }
}
