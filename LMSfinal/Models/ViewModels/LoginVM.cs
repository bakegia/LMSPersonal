using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.ViewModels
{
    public class LoginVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "hãy nhập tên")]
        public string UserName { get; set; }

        [DataType(DataType.Password), Required(ErrorMessage = " hãy nhập đúng yêu cầu mật khẩu")]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
