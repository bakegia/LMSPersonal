using System.ComponentModel.DataAnnotations;
using LMSfinal.Models;

namespace LMSfinal.Models.EF
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string RecipientUserId { get; set; } = string.Empty;

        public ApplicationUser? RecipientUser { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }
    }
}