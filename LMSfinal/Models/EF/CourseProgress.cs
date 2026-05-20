namespace LMSfinal.Models.EF
{
    public class CourseProgress
    {
        public int Id { get; set; }

        // Liên kết với Sinh viên
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        // Liên kết với Lớp học (Classroom) 
        // Lưu ý: Nên theo dõi theo Classroom vì mỗi lớp có lịch trình riêng
        public int ClassroomId { get; set; }
        public virtual Classroom Classroom { get; set; }

        // Các thông số tiến độ
        public int CompletedLessonsCount { get; set; } // Số bài đã học xong
        public int TotalLessonsCount { get; set; }      // Tổng số bài trong lớp đó

        // Phần trăm hoàn thành (có thể dùng thuộc tính tính toán hoặc lưu trực tiếp)
        public double ProgressPercentage => TotalLessonsCount > 0
            ? (double)CompletedLessonsCount / TotalLessonsCount * 100
            : 0;

        public DateTime LastAccessed { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; } = false; // Đã xong 100% chưa
    }
}
