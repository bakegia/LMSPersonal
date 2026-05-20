using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSfinal.Migrations
{
    /// <inheritdoc />
    public partial class _1305 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Quizzes",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "LessonId", "PassingScore", "Title" },
                values: new object[] { 1, new DateTime(2026, 5, 13, 16, 25, 7, 807, DateTimeKind.Local).AddTicks(909), null, true, 1, 70, "Quiz - Bài 1" });
        }
    }
}
