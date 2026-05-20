using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSfinal.Models.EF
{
    public class Instructor
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(50)]
        public string SpecializedSubject { get; set; }

        public string? ProfileImageUrl { get; set; }

        [StringLength(20)]
        public string EmployeeCode { get; set; }

        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Foreign Key với ApplicationUser
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        // Navigation - Danh sách khóa học[NotMapped]
        [NotMapped]
        public IFormFile ImageUpload { get; set; }
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}