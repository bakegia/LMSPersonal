using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LMSfinal.Models.Enums;

namespace LMSfinal.Models.EF
{
    /// <summary>
    /// Mô hình lưu trữ điểm của học sinh trong lớp học
    /// Gồm: Điểm quá trình (30%), Điểm giữa kỳ (20%), Điểm thi (50%)
    /// </summary>
    public class ClassroomGrade
    {
        [Key]
        public int Id { get; set; }

        // Foreign Keys
        [ForeignKey(nameof(Classroom))]
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }

        [ForeignKey(nameof(Student))]
        [Required(ErrorMessage = "Vui lòng chọn học sinh")]
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser? Student { get; set; }

        // ==================== ĐIỂM THÀNH PHẦN ====================
        /// <summary>
        /// Điểm quá trình (trọng số: 30%)
        /// </summary>
        [Range(0, 10, ErrorMessage = "Điểm quá trình phải từ 0 đến 10")]
        public decimal? ProcessScore { get; set; }

        /// <summary>
        /// Điểm giữa kỳ (trọng số: 20%)
        /// </summary>
        [Range(0, 10, ErrorMessage = "Điểm giữa kỳ phải từ 0 đến 10")]
        public decimal? MidtermScore { get; set; }

        /// <summary>
        /// Điểm thi (trọng số: 50%)
        /// </summary>
        [Range(0, 10, ErrorMessage = "Điểm thi phải từ 0 đến 10")]
        public decimal? FinalExamScore { get; set; }

        // ==================== ĐIỂM TÍNH TOÁN ====================
        /// <summary>
        /// Điểm tổng (tính tự động từ 3 thành phần)
        /// Công thức: (ProcessScore * 0.3) + (MidtermScore * 0.2) + (FinalExamScore * 0.5)
        /// </summary>
        [Range(0, 10)]
        public decimal FinalScore { get; set; } = 0;

        /// <summary>
        /// GPA (tính từ điểm tổng, thang điểm 0-4)
        /// Công thức: (FinalScore / 10) * 4
        /// </summary>
        [Range(0, 4)]
        public decimal GPA { get; set; } = 0;

        /// <summary>
        /// Hạng điểm (A, B, C, D, F)
        /// A: 8.5-10 | B: 7.0-8.4 | C: 5.5-6.9 | D: 4.0-5.4 | F: <4.0
        /// </summary>
        [Required(ErrorMessage = "Vui lòng xác định hạng điểm")]
        public GradeLetterEnum GradeLetterClass { get; set; } = GradeLetterEnum.F;

        // ==================== METADATA ====================
        /// <summary>
        /// ID của giáo viên chấm điểm
        /// </summary>
        [ForeignKey(nameof(GradedByInstructor))]
        public string? GradedByInstructorId { get; set; }
        public ApplicationUser? GradedByInstructor { get; set; }

        /// <summary>
        /// Ghi chú/nhận xét từ giáo viên
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Comments { get; set; }

        /// <summary>
        /// Ngày tạo bản ghi
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Ngày cập nhật bản ghi
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
    }
}