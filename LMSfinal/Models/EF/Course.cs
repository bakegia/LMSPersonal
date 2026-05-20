using LMSfinal.Data.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LMSfinal.Models.EF
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = null!;
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public int? Quantity { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }


        // Navigation
        public ICollection<Section> Sections { get; set; } = new List<Section>();

        public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
        [NotMapped]
        [FileExtensionAttribute]
        public IFormFile? ImageUpload { get; set; }
    }
}

