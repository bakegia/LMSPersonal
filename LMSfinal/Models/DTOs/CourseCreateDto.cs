using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.DTOs
{
    public class CourseCreateDto
    {
        [Required(ErrorMessage = "Course Code is required")]
        public string CourseCode { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Slug is required")]
        public string Slug { get; set; }

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }

        public IFormFile? ImageUpload { get; set; }
    }
}