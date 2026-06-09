using System.ComponentModel.DataAnnotations;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMSfinal.Models.ViewModels.Student
{
    public class StudentPreferenceInput
    {
        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ca học")]
        public int TimeSlotId { get; set; }

        public string? InstructorId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(1000, ErrorMessage = "Lý do không vượt quá 1000 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }

    public class StudentPreferencePageViewModel
    {
        public List<Classroom> Classrooms { get; set; } = new();

        public SelectList Courses { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public SelectList TimeSlots { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public SelectList Instructors { get; set; } = new(Enumerable.Empty<SelectListItem>());

        public StudentPreferenceInput Input { get; set; } = new();
        public List<StudentPreference> Preferences { get; internal set; }
    }
}