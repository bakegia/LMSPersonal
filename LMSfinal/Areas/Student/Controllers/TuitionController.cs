using LMSfinal.Data;
using LMSfinal.Models.Enums;
using LMSfinal.Models.ViewModels.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    public class TuitionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TuitionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? semester)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var enrollments = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == userId)
                .Include(cs => cs.Classroom)
                .ToListAsync();

            if (!enrollments.Any())
            {
                return View(new StudentTuitionIndexVM());
            }

            var classroomIds = enrollments.Select(x => x.ClassroomId).Distinct().ToList();

            var payments = await _context.ClassroomPayments
                .Where(p => p.StudentId == userId && classroomIds.Contains(p.ClassroomId))
                .ToListAsync();

            var itemsAll = enrollments
                .GroupBy(e => new
                {
                    e.Classroom.Semester,
                    e.Classroom.StartDate,
                    e.Classroom.EndDate
                })
                .Select(g =>
                {
                    var semesterLabel = !string.IsNullOrWhiteSpace(g.Key.Semester)
                        ? g.Key.Semester
                        : $"{g.Key.StartDate:MM/yyyy} - {g.Key.EndDate:MM/yyyy}";

                    var classIds = g.Select(x => x.ClassroomId).ToList();

                    var baseTuition = g.Sum(x => x.TotalPrice);
                    var discount = 0m;
                    var paid = payments
                        .Where(p => classIds.Contains(p.ClassroomId) && p.Status == PaymentStatus.Paid)
                        .Sum(p => p.Amount);

                    return new TuitionSemesterItemVM
                    {
                        SemesterLabel = semesterLabel,
                        BaseTuition = baseTuition,
                        Discount = discount,
                        Paid = paid
                    };
                })
                .OrderBy(x => x.SemesterLabel)
                .ToList();

            var semesterOptions = itemsAll
                .Select(x => x.SemesterLabel)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var items = string.IsNullOrWhiteSpace(semester)
                ? itemsAll
                : itemsAll.Where(x => x.SemesterLabel == semester).ToList();

            return View(new StudentTuitionIndexVM
            {
                Items = items,
                SemesterOptions = semesterOptions,
                SelectedSemester = semester
            });
        }
    }
}