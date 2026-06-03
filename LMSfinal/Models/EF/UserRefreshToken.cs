using Microsoft.AspNetCore.Identity;

namespace LMSfinal.Models.EF
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; } // Map với Jti của Access Token để kiểm tra tính hợp lệ hợp phần
        public bool IsRevoked { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
