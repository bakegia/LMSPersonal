using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSfinal.Models.EF
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài tập")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [StringLength(5000)]
        public string? Content { get; set; } // Nội dung chi tiết, hướng dẫn

        [Required(ErrorMessage = "Vui lòng chọn hạn chót")]
        public DateTime DueDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey(nameof(Classroom))]
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }

        [ForeignKey(nameof(Instructor))]
        public string? InstructorId { get; set; }
        public ApplicationUser? Instructor { get; set; }

        // Navigation
        public ICollection<StudentAssignment> StudentAssignments { get; set; } = new List<StudentAssignment>();
    }

    public class StudentAssignment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Assignment))]
        public int AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }

        [ForeignKey(nameof(Student))]
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser? Student { get; set; }

        public bool IsSubmitted { get; set; } = false;
        public DateTime? FirstSubmittedAt { get; set; }
        public DateTime? LastSubmittedAt { get; set; }

        public string? Score { get; set; } // "8.5", "Excellent", etc.
        public string? Feedback { get; set; } // Nhận xét từ giáo viên
        public DateTime? GradedDate { get; set; }

        // Navigation
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }

    public class AssignmentSubmission
    {
        public int Id { get; set; }

        [ForeignKey(nameof(StudentAssignment))]
        public int StudentAssignmentId { get; set; }
        public StudentAssignment? StudentAssignment { get; set; }

        [StringLength(5000)]
        public string SubmissionText { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; } // URL file đính kèm
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public bool IsLate { get; set; } = false; // Nộp trễ hay không
    }
}