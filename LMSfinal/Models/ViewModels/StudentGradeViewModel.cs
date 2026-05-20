using LMSfinal.Models.EF;

namespace LMSfinal.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho danh sách điểm của học sinh
    /// </summary>
    public class StudentGradeItemVM
    {
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public string ClassroomCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ClassroomGrade? Grade { get; set; }
    }

    /// <summary>
    /// ViewModel cho view Index của Grade
    /// </summary>
    public class StudentGradeIndexVM
    {
        public List<StudentGradeItemVM> Items { get; set; } = new();
        public int TotalCourses => Items.Count;
        public int GradedCourses => Items.Count(x => x.Grade != null);
        public int UngradedCourses => Items.Count(x => x.Grade == null);
        public decimal AverageScore => Items.Where(x => x.Grade != null).Any()
            ? Items.Where(x => x.Grade != null).Average(x => x.Grade.FinalScore)
            : 0m;
    }
}