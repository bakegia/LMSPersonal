using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSfinal.Migrations
{
    /// <inheritdoc />
    public partial class assignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProgress_AspNetUsers_UserId",
                table: "UserProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgress_Lessons_LessonId",
                table: "UserProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProgress",
                table: "UserProgress");

            migrationBuilder.RenameTable(
                name: "UserProgress",
                newName: "UserProgresses");

            migrationBuilder.RenameIndex(
                name: "IX_UserProgress_UserId",
                table: "UserProgresses",
                newName: "IX_UserProgresses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProgress_LessonId",
                table: "UserProgresses",
                newName: "IX_UserProgresses_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProgresses",
                table: "UserProgresses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClassroomId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assignments_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    FirstSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GradedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAssignment_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAssignment_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentSubmission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentAssignmentId = table.Column<int>(type: "int", nullable: false),
                    SubmissionText = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSubmission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmission_StudentAssignment_StudentAssignmentId",
                        column: x => x.StudentAssignmentId,
                        principalTable: "StudentAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassroomId",
                table: "Assignments",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_InstructorId",
                table: "Assignments",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmission_StudentAssignmentId",
                table: "AssignmentSubmission",
                column: "StudentAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignment_AssignmentId",
                table: "StudentAssignment",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignment_StudentId",
                table: "StudentAssignment",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgresses_AspNetUsers_UserId",
                table: "UserProgresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgresses_Lessons_LessonId",
                table: "UserProgresses",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProgresses_AspNetUsers_UserId",
                table: "UserProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgresses_Lessons_LessonId",
                table: "UserProgresses");

            migrationBuilder.DropTable(
                name: "AssignmentSubmission");

            migrationBuilder.DropTable(
                name: "StudentAssignment");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProgresses",
                table: "UserProgresses");

            migrationBuilder.RenameTable(
                name: "UserProgresses",
                newName: "UserProgress");

            migrationBuilder.RenameIndex(
                name: "IX_UserProgresses_UserId",
                table: "UserProgress",
                newName: "IX_UserProgress_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProgresses_LessonId",
                table: "UserProgress",
                newName: "IX_UserProgress_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProgress",
                table: "UserProgress",
                column: "Id");

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
    }
}
