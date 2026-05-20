namespace LMSfinal.Models.EF
{
    public class ClassroomStudent
    {
        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
        public bool IsPresent { get; internal set; }
    }
}
