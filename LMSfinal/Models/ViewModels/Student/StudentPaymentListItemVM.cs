using LMSfinal.Models.Enums;

namespace LMSfinal.Models.ViewModels.Student
{
    public class StudentPaymentListItemVM
    {
        public int PaymentId { get; set; }
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class StudentPaymentLockedViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public PaymentStatus Status { get; set; }
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
    }
}