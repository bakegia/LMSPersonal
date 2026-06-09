using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.EF
{
    public class StudentPreference
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        public ApplicationUser? Student { get; set; }

        public int CourseId { get; set; }

        public Course? Course { get; set; }

        public int TimeSlotId { get; set; }

        public TimeSlot? TimeSlot { get; set; }

        public string? InstructorId { get; set; }

        public ApplicationUser? Instructor { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}