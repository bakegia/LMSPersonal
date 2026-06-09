namespace LMSfinal.Models.EF
{
    public class ClassroomStudent
    {
        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
        public bool IsPresent { get; internal set; }

        public DateTime EnrolledAt { get; set; } = DateTime.Now;
        public decimal PricePerCreditAtEnroll { get; set; }
        public decimal TotalPrice { get; set; }

        public bool IsLocked { get; set; }
        public DateTime? LockedAt { get; set; }
        public string? LockReason { get; set; }
    }
}