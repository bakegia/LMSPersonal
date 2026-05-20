using LMSfinal.Data.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSfinal.Models.EF
{
    public class UserProfile
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; }
        [Required]
        public int Mssv { get; set; }
        public string Name { get; set; }
        [Required]
        public string Fullname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        public string Address { get; set; }
        public string? ProfilePictureUrl { get; set; }
        [NotMapped]
        [FileExtensionAttribute]   
        public IFormFile? Imageupload { get; set; }
        public ApplicationUser? User { get; set; }

    }
}
