namespace LMSfinal.Models.EF
{
    public class Attendance
    {
        public int Id { get; set; }

        public int ClassroomId { get; set; }
        public virtual Classroom Classroom { get; set; }

        public string StudentId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public bool IsPresent { get; set; }

        public bool IsLate { get; set; }

        public string? Note { get; set; }
    }
}
