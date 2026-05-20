using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSfinal.Migrations
{
    /// <inheritdoc />
    public partial class giangvienprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 5, 13, 16, 25, 7, 807, DateTimeKind.Local).AddTicks(909));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 5, 13, 15, 41, 24, 61, DateTimeKind.Local).AddTicks(81));
        }
    }
}
