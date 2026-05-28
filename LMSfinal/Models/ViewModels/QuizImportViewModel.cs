using LMSfinal.Models.Imports;
using Microsoft.AspNetCore.Http;

namespace LMSfinal.Models.ViewModels.Instructor
{
    public class QuizImportViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
        public QuizImportResult? Result { get; set; }
    }
}