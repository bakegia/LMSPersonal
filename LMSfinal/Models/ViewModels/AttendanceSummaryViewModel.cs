namespace LMSfinal.Models.ViewModels
{
    public class AttendanceSummaryViewModel
    {
        public int? SelectedClassroomId { get; set; }
        public string? ClassroomName { get; set; }
        public int TotalSessions { get; set; }
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public List<ClassroomOptionVM> Classrooms { get; set; } = new();
        public List<AttendanceSummaryStudentVM> Students { get; set; } = new();
        public List<AttendanceDateOptionVM> AvailableDates { get; set; } = new();
    }

    public class ClassroomOptionVM
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class AttendanceSummaryStudentVM
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int NotTakenCount { get; set; }
        public double AttendanceRate { get; set; }
    }
}
