namespace LMSfinal.Models.EF
{
    public class UserProgress
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int LessonId { get; set; }
        public virtual Lesson Lesson { get; set; } 

        public DateTime CompletedDate { get; set; } = DateTime.Now;

    }

}