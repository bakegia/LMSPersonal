using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.ViewModels
{
    public class CreateInstructorVM
    {
        [Required(ErrorMessage = "Tên tài kho?n là b?t bu?c")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Tên ð?y ð? là b?t bu?c")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "M?t kh?u ph?i có ít nh?t 6 k? t?")]
        public string Password { get; set; }

        [Display(Name = "S? ði?n tho?i")]
        [Phone(ErrorMessage = "S? ði?n tho?i không h?p l?")]
        [Required(ErrorMessage = "S? ði?n tho?i là b?t bu?c")]
        public string PhoneNumber { get; set; }

        [Display(Name = "B? môn/Khoa")]
        [Required(ErrorMessage = "B? môn/Khoa là b?t bu?c")]
        [StringLength(100)]
        public string Department { get; set; }

        [Display(Name = "Môn h?c chuyên trách")]
        [Required(ErrorMessage = "Môn h?c chuyên trách là b?t bu?c")]
        [StringLength(50)]
        public string SpecializedSubject { get; set; }

        [Display(Name = "M? Nhân Viên")]
        [StringLength(20)]
        public string EmployeeCode { get; set; }

        [Display(Name = "Ngày B?t Ð?u Làm Vi?c")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Ngày b?t ð?u làm vi?c là b?t bu?c")]
        public DateTime HireDate { get; set; }

        [Required]
        public string RoleId { get; set; }
    }
}