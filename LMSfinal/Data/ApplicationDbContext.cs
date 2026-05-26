using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LMSfinal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ClassroomStudent> ClassroomStudents { get; set; }  
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ClassroomGrade> ClassroomGrades { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========================= USER PROGRESS =========================
            builder.Entity<UserProgress>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProgresses)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================= CLASSROOM STUDENT =========================
            builder.Entity<ClassroomStudent>()
                .HasKey(cs => new { cs.ClassroomId, cs.StudentId });

            builder.Entity<ClassroomStudent>()
                .HasOne(cs => cs.Classroom)
                .WithMany(c => c.ClassroomStudents)
                .HasForeignKey(cs => cs.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassroomStudent>()
                .HasOne(cs => cs.Student)
                .WithMany()
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================= SECTION =========================
            builder.Entity<Section>()
                .HasOne(s => s.Classroom)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Section>()
                .HasIndex(s => new { s.ClassroomId, s.Order })
                .IsUnique();

            // ========================= LESSON =========================
            builder.Entity<Lesson>()
                .HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Lesson>()
                .HasIndex(l => new { l.SectionId, l.Order })
                .IsUnique();

            // ========================= QUIZ CONFIGURATION =========================

            // Quiz -> Lesson
            builder.Entity<Quiz>()
                .HasOne(q => q.Lesson)
                .WithMany()
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> QuizQuestion (1 - Many)
            builder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizQuestion -> QuizAnswer (1 - Many)
            builder.Entity<QuizAnswer>()
                .HasOne(qa => qa.Question)
                .WithMany(qq => qq.Answers)
                .HasForeignKey(qa => qa.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> StudentQuizAttempt (1 - Many)
            builder.Entity<StudentQuizAttempt>()
                .HasOne(sa => sa.Quiz)
                .WithMany(q => q.StudentAttempts)
                .HasForeignKey(sa => sa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentQuizAttempt -> Student (ApplicationUser)
            builder.Entity<StudentQuizAttempt>()
                .HasOne(sa => sa.Student)
                .WithMany() // Nếu ApplicationUser chưa có ICollection<StudentQuizAttempt>
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentQuizAttempt -> StudentQuizAnswer (1 - Many)
            builder.Entity<StudentQuizAnswer>()
                .HasOne(sqa => sqa.Attempt)
                .WithMany(sa => sa.Answers)
                .HasForeignKey(sqa => sqa.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hạn chế Cascade để tránh Multiple Cascade Paths
            builder.Entity<StudentQuizAnswer>()
                .HasOne(sqa => sqa.Question)
                .WithMany(qq => qq.StudentAnswers)
                .HasForeignKey(sqa => sqa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentQuizAnswer>()
                .HasOne(sqa => sqa.SelectedAnswer)
                .WithMany(qa => qa.StudentAnswers)
                .HasForeignKey(sqa => sqa.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chỉ số và Ràng buộc
            builder.Entity<StudentQuizAttempt>()
                .HasIndex(x => new { x.StudentId, x.QuizId });

            builder.Entity<StudentQuizAttempt>()
                .Property(x => x.Score).HasPrecision(18, 2);
            builder.Entity<StudentQuizAttempt>()
                .Property(x => x.TotalPoints).HasPrecision(18, 2);

            builder.Entity<QuizQuestion>()
                .HasIndex(x => new { x.QuizId, x.Order });

            builder.Entity<QuizAnswer>()
                .HasIndex(x => new { x.QuestionId, x.Order });

            // ========================= CATEGORY =========================
            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(x => x.CategoryCode)
                    .IsUnique();

                entity.Property(x => x.CategoryCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Name)
                    .HasMaxLength(200)
                    .IsRequired();
            });

            // ========================= COURSE =========================
            builder.Entity<Course>(entity =>
            {
                entity.HasIndex(x => x.CourseCode)
                    .IsUnique();

                entity.Property(x => x.CourseCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Title)
                    .HasMaxLength(250)
                    .IsRequired();

                // Nếu có Slug
                // entity.HasIndex(x => x.Slug).IsUnique();
            });

            // ========================= CLASSROOM =========================
            builder.Entity<Classroom>(entity =>
            {
                entity.HasIndex(x => x.ClassCode)
                    .IsUnique();

                entity.Property(x => x.ClassCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.NameClass)
                    .HasMaxLength(200)
                    .IsRequired();
            });
        }

    }
}
