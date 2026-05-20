using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.EF
{
    public class Contact
    {
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "New";
    }
}