using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LMSfinal.Models.EF
{
    public class Section
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề chương")]
        public string Title { get; set; }
        public int Order { get; set; } // Thứ tự hiển thị

        public int ClassroomId { get; set; }
        [JsonIgnore]
        public Classroom Classroom { get; set; }

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }

    public class Lesson
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        [Required]
        public string Title { get; set; }
        public string? VideoUrl { get; set; } // Link Youtube/Drive
        [StringLength(500)]
        public string? Summary { get; set; }
        public string? Content { get; set; }  // Nội dung văn bản
        public int Order { get; set; }

        public int SectionId { get; set; }
        [JsonIgnore]
        public Section? Section { get; set; }
        [NotMapped]
        public IFormFile? VideoUpload { get; set; } // upload file
        public ICollection<Quiz> Quizzes { get; set; }
        public bool? IsPreviewFree { get; set; }
    }

}
