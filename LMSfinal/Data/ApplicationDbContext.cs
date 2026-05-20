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

            // ========================= QUIZ =========================

            // Quiz → QuizQuestion
            builder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizQuestion → QuizAnswer
            builder.Entity<QuizQuestion>()
                .HasMany(qq => qq.Answers)
                .WithOne(qa => qa.Question)
                .HasForeignKey(qa => qa.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz → StudentQuizAttempt
            builder.Entity<Quiz>()
                .HasMany(q => q.StudentAttempts)
                .WithOne(squa => squa.Quiz)
                .HasForeignKey(squa => squa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentQuizAttempt → StudentQuizAnswer
            builder.Entity<StudentQuizAttempt>()
                .HasMany(squa => squa.Answers)
                .WithOne(sqa => sqa.Attempt)
                .HasForeignKey(sqa => sqa.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizQuestion → StudentQuizAnswer
            builder.Entity<QuizQuestion>()
                .HasMany(qq => qq.StudentAnswers)
                .WithOne(sqa => sqa.Question)
                .HasForeignKey(sqa => sqa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizAnswer → StudentQuizAnswer
            builder.Entity<QuizAnswer>()
                .HasMany(qa => qa.StudentAnswers)
                .WithOne(sqa => sqa.SelectedAnswer)
                .HasForeignKey(sqa => sqa.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nếu quiz chỉ được làm 1 lần
            builder.Entity<StudentQuizAttempt>()
                .HasIndex(x => new
                {
                    x.StudentId,
                    x.QuizId
                })
                .IsUnique();

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

            // ========================= OPTIONAL FUTURE CONFIG =========================

            /*
            // Attendance unique
            builder.Entity<Attendance>()
                .HasIndex(x => new
                {
                    x.StudentId,
                    x.ScheduleId,
                    x.Date
                })
                .IsUnique();
            */

            /*
            // Enrollment unique
            builder.Entity<Enrollment>()
                .HasIndex(x => new
                {
                    x.StudentId,
                    x.CourseId
                })
                .IsUnique();
            */
        }

    }
}
