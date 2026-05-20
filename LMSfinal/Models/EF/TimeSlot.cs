using System.Text.Json.Serialization;

namespace LMSfinal.Models.EF
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        [JsonIgnore]
        public ICollection<ClassSchedule>? ClassSchedules { get; set; }

    }
}
