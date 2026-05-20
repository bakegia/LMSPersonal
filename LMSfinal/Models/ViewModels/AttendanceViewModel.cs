namespace LMSfinal.Models.ViewModels
{
    public class AttendanceViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime? SelectedDate { get; set; }
        public string SelectedWeekKey { get; set; } = string.Empty;
        public List<WeekOptionVM> WeekOptions { get; set; } = new();
        public List<AttendanceDateOptionVM> AvailableDates { get; set; } = new();

        public List<StudentAttendanceVM> Students { get; set; } = new();
    }

    public class StudentAttendanceVM
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool? IsPresent { get; set; } // null = chưa điểm danh
    }

    public class AttendanceDateOptionVM
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class WeekOptionVM
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
