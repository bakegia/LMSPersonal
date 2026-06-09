using LMSfinal.Models.Enums;

namespace LMSfinal.Models.Student
{
    public class StudentPaymentListItemVM
    {
        public int BillId { get; set; }

        public string ClassroomName { get; set; }

        public string CourseTitle { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
