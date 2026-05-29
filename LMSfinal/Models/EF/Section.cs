using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LMSfinal.Models.EF
{
    public class Section
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề chương")]
        public string Title { get; set; } = string.Empty;

        [Range(1, 9999, ErrorMessage = "Thứ tự phải lớn hơn 0")]
        public int Order { get; set; } // Thứ tự hiển thị

        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int ClassroomId { get; set; }

        [JsonIgnore]
        public Classroom Classroom { get; set; }

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }

    public class Lesson
    {
        public int Id { get; set; }

        [StringLength(200)]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài học")]
        public string Title { get; set; } = string.Empty;

        public string? VideoUrl { get; set; } // Link Youtube/Drive

        [StringLength(500)]
        public string? Summary { get; set; }

        public string? Content { get; set; }  // Nội dung văn bản

        [Range(1, 9999, ErrorMessage = "Thứ tự phải lớn hơn 0")]
        public int Order { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chương")]
        public int SectionId { get; set; }

        [JsonIgnore]
        public Section? Section { get; set; }

        // ===== Completion Rules =====
        public int? VideoDurationSeconds { get; set; } // Tổng thời lượng video (giây)
        
        [Range(1, 100)]
        public int RequiredWatchPercent { get; set; } = 90; // % cần xem

        public bool RequireQuiz { get; set; } = true;
        public bool RequireQuizPass { get; set; } = true;

        [NotMapped]
        public IFormFile? VideoUpload { get; set; } // upload file

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public bool IsPreviewFree { get; set; } = false;
    }
}
