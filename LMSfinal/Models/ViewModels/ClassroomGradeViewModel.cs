using System.ComponentModel.DataAnnotations;
using LMSfinal.Models.Enums;

namespace LMSfinal.Models.ViewModels
{
    /// <summary>
    /// ViewModel để hiển thị danh sách học sinh cần nhập điểm
    /// </summary>
    public class ClassroomGradeListViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassroomCode { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public List<StudentGradeItemViewModel> StudentGrades { get; set; } = new();
    }

    /// <summary>
    /// ViewModel cho từng học sinh trong danh sách
    /// </summary>
    public class StudentGradeItemViewModel
    {
        public int GradeId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;

        public decimal? ProcessScore { get; set; }      // 30%
        public decimal? MidtermScore { get; set; }      // 20%
        public decimal? FinalExamScore { get; set; }    // 50%

        public decimal FinalScore { get; set; }
        public decimal GPA { get; set; }
        public GradeLetterEnum GradeLetterClass { get; set; }

        public string? Comments { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// ViewModel để nhập/sửa điểm của một học sinh
    /// </summary>
    public class GradeInputViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int ClassroomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn học sinh")]
        public string StudentId { get; set; } = string.Empty;

        public int GradeId { get; set; } // 0 nếu tạo mới

        // Student Info (chỉ để hiển thị)
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;

        // Điểm nhập vào
        [Range(0, 10, ErrorMessage = "Điểm quá trình phải từ 0 đến 10")]
        [Display(Name = "Điểm Quá Trình (30%)")]
        public decimal? ProcessScore { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm giữa kỳ phải từ 0 đến 10")]
        [Display(Name = "Điểm Giữa Kỳ (20%)")]
        public decimal? MidtermScore { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm thi phải từ 0 đến 10")]
        [Display(Name = "Điểm Thi (50%)")]
        public decimal? FinalExamScore { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi Chú")]
        public string? Comments { get; set; }

        // Kết quả tính toán (chỉ để hiển thị)
        public decimal FinalScore { get; set; }
        public decimal GPA { get; set; }
        public GradeLetterEnum GradeLetterClass { get; set; }
    }

    /// <summary>
    /// ViewModel để hiển thị chi tiết điểm của một học sinh
    /// </summary>
    public class GradeDetailViewModel
    {
        public int GradeId { get; set; }
        public int ClassroomId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public decimal? ProcessScore { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalExamScore { get; set; }

        public decimal FinalScore { get; set; }
        public decimal GPA { get; set; }
        public GradeLetterEnum GradeLetterClass { get; set; }
        public string GradeDescription { get; set; } = string.Empty;

        public string? Comments { get; set; }
        public string? GradedByInstructorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}