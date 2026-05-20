using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Data.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName); //123.png
                string[] extensions = { "jpg", "png", "jpeg", "mov", "mp3", "pdf", "mp4","doc", "excel", "pptx" , "zip", "gif" };
                bool result = extension.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Allowed extensions are jpg, jpeg, mov, mp3, pdf, mp4, doc, excel, pptx, zip, gif");
                }
            }

            return ValidationResult.Success;
        }
    }
}
