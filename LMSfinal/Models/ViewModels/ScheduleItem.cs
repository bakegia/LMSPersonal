namespace LMSfinal.Models.ViewModels
{
    public class ScheduleItem
    {
        public DayOfWeek Day { get; set; }   // Thứ
        public int PeriodStart { get; set; } // Tiết bắt đầu
        public int PeriodEnd { get; set; }   // Tiết kết thúc

        public string Subject { get; set; }
        public string Room { get; set; }
        public string Teacher { get; set; }
    }
}
