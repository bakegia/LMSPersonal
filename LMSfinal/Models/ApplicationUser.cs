using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity;

namespace LMSfinal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool? IsActive { get; set; }
        public string? ResetCode { get; set; } 
        public DateTime? ResetCodeExpiration { get; set; } 
        public List<UserProgress> UserProgresses { get; set; } = new();
            
    }
}
