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
        public DbSet<FinalExamSchedule> FinalExamSchedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StudentPreference> StudentPreferences { get; set; }
        public DbSet<PricePerCreditHistory> PricePerCreditHistories { get; set; }
        public DbSet<ClassroomPayment> ClassroomPayments { get; set; }

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

            builder.Entity<ClassroomStudent>()
                .Property(cs => cs.PricePerCreditAtEnroll)
                .HasPrecision(18, 2);

            builder.Entity<ClassroomStudent>()
                .Property(cs => cs.TotalPrice)
                .HasPrecision(18, 2);

            // ========================= PRICE PER CREDIT =========================
            builder.Entity<PricePerCreditHistory>(entity =>
            {
                entity.Property(x => x.Price).HasPrecision(18, 2);
                entity.HasIndex(x => new { x.EffectiveFrom, x.EffectiveTo });
                entity.HasData(new PricePerCreditHistory
                {
                    Id = 1,
                    Price = 2500000m,
                    EffectiveFrom = new DateTime(2026, 5, 28),
                    EffectiveTo = null
                });
            });

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
                .WithMany()
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

            // ========================= NOTIFICATION =========================
            builder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.RecipientUser)
                    .WithMany()
                    .HasForeignKey(n => n.RecipientUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(n => new { n.RecipientUserId, n.IsRead, n.CreatedAt });
            });

            // ========================= STUDENT PREFERENCE =========================
            builder.Entity<StudentPreference>(entity =>
            {
                entity.Property(x => x.Reason)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.HasOne(x => x.Student)
                    .WithMany()
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Instructor)
                    .WithMany()
                    .HasForeignKey(x => x.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Course)
                    .WithMany()
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TimeSlot)
                    .WithMany()
                    .HasForeignKey(x => x.TimeSlotId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.StudentId, x.CreatedAt });
            });

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

                entity.Property(x => x.Credits)
                    .HasDefaultValue(0);
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
            builder.Entity<FinalExamSchedule>(entity =>
            {
                entity.HasOne(e => e.Classroom)
                      .WithMany()
                      .HasForeignKey(e => e.ClassroomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // ========================= CLASSROOM PAYMENT =========================
            builder.Entity<ClassroomPayment>(entity =>
            {
                entity.Property(x => x.Amount).HasPrecision(18, 2);

                entity.HasIndex(x => new { x.StudentId, x.ClassroomId })
                    .IsUnique();

                entity.HasOne(x => x.Classroom)
                    .WithMany()
                    .HasForeignKey(x => x.ClassroomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Student)
                    .WithMany()
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

