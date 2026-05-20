using LMSfinal.Models.EF;

namespace LMSfinal.Models.ViewModels
{
    public class WeeklyScheduleViewModel
    {
        public List<TimeSlot> TimeSlots { get; set; }
        public List<ClassSchedule> Schedules { get; set; }
        public DateTime WeekStart { get; set; }
        public List<DateTime> DaysInWeek { get; set; }
    }
}
