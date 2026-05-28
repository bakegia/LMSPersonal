namespace LMSfinal.Models.ViewModels
{
    public class AttendanceSubmitModel
    {
        public string StudentId { get; set; }
        public bool IsPresent { get; set; }
        public bool IsLate { get; set; }
        public string? Note { get; set; }
    }
}
