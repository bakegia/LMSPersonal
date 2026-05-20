using System.Text.Json.Serialization;

namespace LMSfinal.Models.EF
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string? Slug { get; set; }
        public string Name { get; set; }
        public string CategoryCode { get; set; }
        public string? Description { get; set; }
        [JsonIgnore]
        public List<Course>? Courses { get; set; }
    }
}
