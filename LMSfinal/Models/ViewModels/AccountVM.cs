using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.ViewModels
{
    public class AccountVM
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tài khoản là bắt buộc")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc"), EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }

        public string RoleId { get; set; }

        // ============= CHỈ CHO STUDENT =============
        [Display(Name = "Mã sinh viên")]
        public int? Mssv { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        // ============= CHỈ CHO INSTRUCTOR =============
        [Display(Name = "Số điện thoại")]
        public string InstructorPhoneNumber { get; set; }

        [Display(Name = "Bộ môn/Khoa")]
        public string Department { get; set; }

        [Display(Name = "Môn học chuyên trách")]
        public string SpecializedSubject { get; set; }

        [Display(Name = "Mã Nhân Viên")]
        public string EmployeeCode { get; set; }

        [Display(Name = "Ngày Bắt Đầu Làm Việc")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }
    }
}
