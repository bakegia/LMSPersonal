using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSfinal.Migrations
{
    /// <inheritdoc />
    public partial class _1905 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPreviewFree",
                table: "Lessons",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Lessons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPreviewFree",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Lessons");
        }
    }
}
