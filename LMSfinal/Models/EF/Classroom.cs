using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.EF
{
    public class Classroom
    {
        public int Id { get; set; }
        [Required]
        public string ClassCode { get; set; }
        [Required]
        public string NameClass { get; set; }
        public string? NumberClass { get; set; }
        [Required]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string? InstructorId { get; set; }
        public ApplicationUser? Instructor { get; set; }
        //public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } 
        //public virtual ICollection<ClassroomStudent> ClassroomStudents { get; set; }
        public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
