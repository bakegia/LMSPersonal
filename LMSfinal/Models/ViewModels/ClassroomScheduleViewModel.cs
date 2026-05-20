namespace LMSfinal.Models.ViewModels
{
    public class ClassroomScheduleViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;

        public string SelectedWeekKey { get; set; } = string.Empty;
        public List<WeekOptionVM> WeekOptions { get; set; } = new();
        public DateTime WeekStartDate { get; set; }

        public List<ScheduleDayColumnVM> DayColumns { get; set; } = new();
        public List<ScheduleTimeSlotRowVM> TimeSlotRows { get; set; } = new();
        public List<ScheduleSessionCellVM> Sessions { get; set; } = new();
    }

    public class ScheduleDayColumnVM
    {
        public DateTime Date { get; set; }
        public string DayLabel { get; set; } = string.Empty;
    }

    public class ScheduleTimeSlotRowVM
    {
        public int TimeSlotId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class ScheduleSessionCellVM
    {
        public int TimeSlotId { get; set; }
        public DateTime Date { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
    }
}
