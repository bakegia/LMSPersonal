using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.ViewModels
{
    public class CreateStudentVM
    {
        [Required(ErrorMessage = "Tên tài khoản là bắt buộc")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Display(Name = "Mã sinh viên")]
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        public int Mssv { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Required]
        public string RoleId { get; set; }
    }
}