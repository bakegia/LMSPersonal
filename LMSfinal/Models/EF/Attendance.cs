namespace LMSfinal.Models.EF
{
    public class Attendance
    {
        public int Id { get; set; }

        public int ClassroomId { get; set; }
        // Thêm dòng này để định nghĩa mối quan hệ
        public virtual Classroom Classroom { get; set; }

        public string StudentId { get; set; }
        // Nếu có thể, hãy thêm liên kết với Student luôn
        // public virtual ApplicationUser Student { get; set; }

        public DateTime AttendanceDate { get; set; }

        public bool IsPresent { get; set; }
    }
}
