using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.ViewModels
{
    public class RegisterVM
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "hãy nhập tên")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "hãy nhập email"), EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = " hãy nhập đúng yêu cầu mật khẩu")]
        public string Password { get; set; }

        public string RoleId { get; set; }
    }
}
