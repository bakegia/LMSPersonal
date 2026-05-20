using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.ViewModels
{
    public class UpdateInstructorVM
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Bộ môn là bắt buộc")]
        public string Department { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Môn học chuyên trách là bắt buộc")]
        public string SpecializedSubject { get; set; }

        [StringLength(20)]
        public string EmployeeCode { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu làm việc là bắt buộc")]
        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Ảnh Đại Diện")]
        public IFormFile ImageUpload { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}