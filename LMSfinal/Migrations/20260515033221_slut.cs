using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSfinal.Migrations
{
    /// <inheritdoc />
    public partial class slut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Classrooms_ClassroomId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_StudentQuizAttempt_StudentId",
                table: "StudentQuizAttempt");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImageUrl",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Courses",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");





            migrationBuilder.AlterColumn<string>(
                name: "NameClass",
                table: "Classrooms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ClassCode",
                table: "Classrooms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "NumberClass",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");





            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizAttempt_StudentId_QuizId",
                table: "StudentQuizAttempt",
                columns: new[] { "StudentId", "QuizId" },
                unique: true);



            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_ClassCode",
                table: "Classrooms",
                column: "ClassCode",
                unique: true);



            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Classrooms_ClassroomId",
                table: "Sections",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Classrooms_ClassroomId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_StudentQuizAttempt_StudentId_QuizId",
                table: "StudentQuizAttempt");



            migrationBuilder.DropIndex(
                name: "IX_Classrooms_ClassCode",
                table: "Classrooms");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CategoryCode",
                table: "Categories");



            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "NumberClass",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "CategoryCode",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImageUrl",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "NameClass",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ClassCode",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizAttempt_StudentId",
                table: "StudentQuizAttempt",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Classrooms_ClassroomId",
                table: "Sections",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
