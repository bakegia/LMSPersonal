using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSfinal.Migrations
{
    /// <inheritdoc />
    public partial class fordiemdanh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserProgress",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_LessonId",
                table: "UserProgress",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_UserId",
                table: "UserProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ClassroomId",
                table: "Attendances",
                column: "ClassroomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Classrooms_ClassroomId",
                table: "Attendances",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgress_AspNetUsers_UserId",
                table: "UserProgress",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgress_Lessons_LessonId",
                table: "UserProgress",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Classrooms_ClassroomId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgress_AspNetUsers_UserId",
                table: "UserProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgress_Lessons_LessonId",
                table: "UserProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserProgress_LessonId",
                table: "UserProgress");

            migrationBuilder.DropIndex(
                name: "IX_UserProgress_UserId",
                table: "UserProgress");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_ClassroomId",
                table: "Attendances");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserProgress",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollment_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseId",
                table: "Enrollment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_UserId",
                table: "Enrollment",
                column: "UserId");
        }
    }
}
