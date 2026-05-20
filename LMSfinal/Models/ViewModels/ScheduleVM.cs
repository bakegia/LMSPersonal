namespace LMSfinal.Models.ViewModels
{
    public class ScheduleVM
    {
        public int TimeSlotId { get; set; }
        public DayOfWeek Day { get; set; }

        public string TimeSlotName { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public string CourseName { get; set; }
        public string ClassName { get; set; }
        public string InstructorName { get; set; }
    }
}
