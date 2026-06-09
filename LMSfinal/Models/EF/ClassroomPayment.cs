using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LMSfinal.Models.Enums;

namespace LMSfinal.Models.EF
{
    public class ClassroomPayment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Classroom))]
        public int ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }

        [ForeignKey(nameof(Student))]
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser? Student { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? PaidAt { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ReminderSentAt { get; set; }
    }
}