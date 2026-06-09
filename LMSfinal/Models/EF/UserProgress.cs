namespace LMSfinal.Models.EF
{
    public class UserProgress
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; }

        public int LessonId { get; set; }
        public virtual Lesson Lesson { get; set; } 

        // ===== Video Progress =====
        public int WatchedSeconds { get; set; }
        public int WatchedPercent { get; set; }
        public bool IsVideoCompleted { get; set; }

        // ===== Quiz Progress =====
        public bool IsQuizPassed { get; set; }
        public DateTime? QuizPassedAt { get; set; }

        // ===== Completion =====
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }

        public DateTime? LastWatchedAt { get; set; }
    }

}