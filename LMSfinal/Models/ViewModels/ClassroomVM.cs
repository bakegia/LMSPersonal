using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ClassroomVM
{
    public int Id { get; set; }

    [Required]
    public string ClassCode { get; set; }

    [Required]
    public string NameClass { get; set; }

    [Required]
    public string Description { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Course
    [ValidateNever]
    public int CourseId { get; set; }

    public List<SelectListItem>? Courses { get; set; }

    // Instructor
    [ValidateNever]
    public string? InstructorId { get; set; }

    public List<SelectListItem>? Instructors { get; set; }

    // MULTI STUDENT (QUAN TRỌNG)
    public List<string> StudentIds { get; set; } = new();
    public List<SelectListItem> Students { get; set; } = new();
    //ca hoc
    [Required(ErrorMessage = "Vui lòng chọn ca học")]
    public int TimeSlotId { get; set; }
    [ValidateNever]
    public List<DayOfWeek>? SelectedDays { get; set; }
    [ValidateNever]
    public List<SelectListItem>? TimeSlots { get; set; }

    public bool IsOpenForRegistration { get; set; } = true;
    public int MaxCapacity { get; set; } = 30;
    public DateTime? RegistrationDeadline { get; set; }
}