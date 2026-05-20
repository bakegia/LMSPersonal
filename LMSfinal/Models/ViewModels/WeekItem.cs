namespace LMSfinal.Models.ViewModels
{
    public class WeekItem
    {
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsSelected { get; set; }
        public string DisplayText => $"Tuần {WeekNumber} [từ {StartDate:dd/MM/yyyy} đến {EndDate:dd/MM/yyyy}]";
    }
}
