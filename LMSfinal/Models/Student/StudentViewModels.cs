namespace LMSfinal.Models.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public int TotalEnrolledCourses { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public double ProgressPercent { get; set; }
        public List<StudentDashboardCourseItemVM> ActiveCourses { get; set; } = new();
        public List<StudentScheduleItemVM> UpcomingSchedule { get; set; } = new();
    }

    public class StudentDashboardCourseItemVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
    }

    public class StudentCourseListItemVM
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EnrolledDate { get; set; }
        public bool IsPaid { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
    }

    public class StudentCourseDetailsViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<StudentSectionLessonsVM> Sections { get; set; } = new();
    }

    public class StudentSectionLessonsVM
    {
        public int SectionId { get; set; }
        public string SectionTitle { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public List<StudentLessonListItemVM> Lessons { get; set; } = new();
    }

    public class StudentLessonListItemVM
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int LessonOrder { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class StudentLessonDetailsViewModel
    {
        public int LessonId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string SectionTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public string? Content { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class StudentAssignmentListItemVM
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? Score { get; set; }
    }

    public class StudentAssignmentDetailsViewModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmissionText { get; set; }
        public string? Score { get; set; }
        public string? Content { get; internal set; }
    }

    public class StudentAssignmentSubmitInput
    {
        public int AssignmentId { get; set; }
        public string SubmissionText { get; set; } = string.Empty;
    }

    public class StudentScheduleViewModel
    {
        public List<StudentScheduleItemVM> Items { get; set; } = new();
    }

    public class StudentScheduleItemVM
    {
        public int ClassroomId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string DayLabel { get; set; } = string.Empty;
        public string TimeSlotName { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
    }

    //public class StudentProfileViewModel
    //{
    //    public string UserId { get; set; } = string.Empty;
    //    public string UserName { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty;
    //    public string FullName { get; set; } = string.Empty;

    //    public int? ProfileId { get; set; }
    //    public int? Mssv { get; set; }
    //    public DateTime? DateOfBirth { get; set; }
    //    public string Gender { get; set; } = string.Empty;
    //    public string PhoneNumber { get; set; } = string.Empty;
    //    public string Address { get; set; } = string.Empty;
    //    public string AvatarUrl { get; set; } = string.Empty;
    //}

    //public class StudentProfileEditInput
    //{
    //    public string FullName { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty;
    //    public int Mssv { get; set; }
    //    public DateTime DateOfBirth { get; set; }
    //    public string Gender { get; set; } = string.Empty;
    //    public string PhoneNumber { get; set; } = string.Empty;
    //    public string Address { get; set; } = string.Empty;
    //}

    public class StudentClassroomListItemVM
    {
        public int ClassroomId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string ScheduleSummary { get; set; } = string.Empty;
    }

    public class StudentClassroomDetailsViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<StudentClassroomScheduleSlotVM> ScheduleSlots { get; set; } = new();
    }

    public class StudentClassroomScheduleSlotVM
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string DayLabel { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
    }

    public class StudentAttendanceViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string SelectedMonthKey { get; set; } = string.Empty;
        public List<StudentMonthOptionVM> MonthOptions { get; set; } = new();
        public int TotalSessions { get; set; }
        public int PresentSessions { get; set; }
        public int AbsentSessions { get; set; }
        public int NotTakenSessions { get; set; }
        public List<StudentAttendanceSessionItemVM> Sessions { get; set; } = new();
    }

    public class StudentAttendanceSessionItemVM
    {
        public DateTime Date { get; set; }
        public string DayLabel { get; set; } = string.Empty;
        public string TimeSlotName { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
        public bool? IsPresent { get; set; }
    }

    public class StudentMonthOptionVM
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    // THÊM VÀO CUỐI FILE (sau StudentMonthOptionVM)

    public class StudentAssignmentIndexViewModel
    {
        public List<StudentAssignmentListItemVM> AllAssignments { get; set; } = new();
        public List<StudentAssignmentListItemVM> UpcomingAssignments { get; set; } = new();
        public List<StudentAssignmentListItemVM> OverdueAssignments { get; set; } = new();
        public List<StudentAssignmentListItemVM> SubmittedAssignments { get; set; } = new();
    }

    public class StudentAssignmentSubmitViewModel
    {
        public int AssignmentId { get; set; }
        public int StudentAssignmentId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Content { get; set; }
        public DateTime DueDate { get; set; }
        public string SubmissionText { get; set; } = string.Empty;
        public IFormFile? AttachmentFile { get; set; }
        public bool HasPreviousSubmission { get; set; } = false;
        public string? PreviousSubmissionText { get; set; }
    }

    public class StudentAssignmentViewDetailViewModel
    {
        public StudentAssignmentDetailsViewModel AssignmentDetail { get; set; }
        public List<StudentAssignmentSubmissionItemVM> Submissions { get; set; } = new();

        public bool IsOverdue { get; set; }
        public bool IsLateSubmission { get; set; }
    }

    public class StudentAssignmentSubmissionItemVM
    {
        public int StudentAssignmentId { get; set; }
        public int SubmissionId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string SubmissionText { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public bool IsLate { get; set; }
    }
}
