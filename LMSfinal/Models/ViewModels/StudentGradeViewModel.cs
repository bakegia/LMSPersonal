using LMSfinal.Models.EF;

namespace LMSfinal.Models.ViewModels
{

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


    public class MonthlyMilestoneVM
    {
        public string MonthYear { get; set; } = string.Empty;
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public DateTime SortDate { get; set; }
    }


    public class StudentGradeIndexVM
    {
        public List<StudentGradeItemVM> Items { get; set; } = new();
        public int TotalCourses => Items.Count;
        public int GradedCourses => Items.Count(x => x.Grade != null);
        public int UngradedCourses => Items.Count(x => x.Grade == null);
        public decimal AverageScore => Items.Where(x => x.Grade != null).Any()
            ? Items.Where(x => x.Grade != null).Average(x => x.Grade.FinalScore)
            : 0m;

        // Thêm mới các thuộc tính thống kê
        public decimal OverallGPA => GradedCourses > 0
            ? Items.Where(x => x.Grade != null).Average(x => x.Grade.GPA)
            : 0m;

        public List<MonthlyMilestoneVM> Milestones { get; set; } = new();
    }

    public class StudentCourseCompletionVM
    {
        public List<StudentGradeItemVM> CompletedCourses { get; set; } = new();
        public List<StudentGradeItemVM> IncompleteCourses { get; set; } = new();

        public int TotalCourses { get; set; }
        public int CompletedCount { get; set; }
        public int IncompleteCount { get; set; }
        public decimal CompletionRate { get; set; } // Tỷ lệ % hoàn thành
    }
}