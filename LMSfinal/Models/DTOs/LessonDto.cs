namespace LMSfinal.Models.DTOs
{
    public class LessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public int Order { get; set; }
        public int SectionId { get; set; }

        public IFormFile? VideoUpload { get; set; } // upload file
    }
}
