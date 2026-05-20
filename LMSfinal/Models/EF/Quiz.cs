using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSfinal.Models.EF
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề Quiz")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "Pass Rate phải từ 0-100")]
        public int PassingScore { get; set; } = 70;

        [ForeignKey(nameof(Lesson))]
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public ICollection<StudentQuizAttempt> StudentAttempts { get; set; } = new List<StudentQuizAttempt>();
    }

    public class QuizQuestion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập câu hỏi")]
        public string QuestionText { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Điểm câu hỏi phải từ 1-4")]
        public int Points { get; set; } = 1;

        public int Order { get; set; }

        [ForeignKey(nameof(Quiz))]
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        // Navigation
        public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
        public ICollection<StudentQuizAnswer> StudentAnswers { get; set; } = new List<StudentQuizAnswer>();
    }

    public class QuizAnswer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án")]
        public string AnswerText { get; set; } = string.Empty;

        public string AnswerLabel { get; set; } = "A"; // A, B, C, D

        public bool IsCorrect { get; set; } = false;

        public int Order { get; set; }

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }
        public QuizQuestion? Question { get; set; }

        // Navigation
        public ICollection<StudentQuizAnswer> StudentAnswers { get; set; } = new List<StudentQuizAnswer>();
    }

    public class StudentQuizAttempt
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser? Student { get; set; }

        [ForeignKey(nameof(Quiz))]
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public decimal Score { get; set; } = 0;

        public decimal TotalPoints { get; set; } = 0;

        public bool Passed { get; set; } = false;

        public DateTime AttemptedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<StudentQuizAnswer> Answers { get; set; } = new List<StudentQuizAnswer>();
    }

    public class StudentQuizAnswer
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Attempt))]
        public int AttemptId { get; set; }
        public StudentQuizAttempt? Attempt { get; set; }

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }
        public QuizQuestion? Question { get; set; }

        [ForeignKey(nameof(SelectedAnswer))]
        public int? SelectedAnswerId { get; set; }
        public QuizAnswer? SelectedAnswer { get; set; }

        public bool IsCorrect { get; set; } = false;

        public int EarnedPoints { get; set; } = 0;
    }
}