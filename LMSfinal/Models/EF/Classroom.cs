using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.EF
{
    public class Classroom
    {
        public int Id { get; set; }
        [Required]
        public string ClassCode { get; set; }
        [Required]
        public string NameClass { get; set; }
        public string? NumberClass { get; set; }
        [Required]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        
        // ==================== ĐĂNG KÝ LỚP ====================
        /// <summary>
        /// Lớp có mở đăng ký không?
        /// </summary>
        public bool IsOpenForRegistration { get; set; } = true;

        /// <summary>
        /// Sức chứa tối đa của lớp
        /// </summary>
        [Range(1, 200, ErrorMessage = "Sức chứa phải từ 1 đến 200")]
        public int MaxCapacity { get; set; } = 30;

        /// <summary>
        /// Hạn cuối để đăng ký (học sinh & giáo viên)
        /// </summary>
        public DateTime? RegistrationDeadline { get; set; }

        // ==================== FOREIGN KEYS ====================
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string? InstructorId { get; set; }
        public ApplicationUser? Instructor { get; set; }

        // ==================== NAVIGATIONS ====================
        public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        // ==================== COMPUTED PROPERTIES ====================
        /// <summary>
        /// Số học sinh đã đăng ký
        /// </summary>
        public int CurrentEnrollment => ClassroomStudents?.Count ?? 0;

        /// <summary>
        /// Lớp đã đủ sức chứa?
        /// </summary>
        public bool IsFull => CurrentEnrollment >= MaxCapacity;

        /// <summary>
        /// Còn bao nhiêu chỗ trống?
        /// </summary>
        public int AvailableSlots => Math.Max(0, MaxCapacity - CurrentEnrollment);

        /// <summary>
        /// Hạn đăng ký đã hết?
        /// </summary>
        public bool IsRegistrationDeadlinePassed => RegistrationDeadline.HasValue && DateTime.Now > RegistrationDeadline.Value;
    }
}
