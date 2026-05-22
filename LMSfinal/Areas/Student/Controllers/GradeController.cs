using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.Enums;
using LMSfinal.Models.Utilities;
using LMSfinal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class GradeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GradeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== INDEX - Danh sách điểm ====================
        /// <summary>
        /// Hiển thị danh sách các lớp và điểm của học sinh
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách lớp của học sinh
            var classrooms = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == userId)
                .Include(cs => cs.Classroom)
                    .ThenInclude(c => c.Course)
                .Select(cs => cs.Classroom)
                .ToListAsync();

            // Lấy danh sách điểm của học sinh
            var grades = await _context.ClassroomGrades
                .Where(g => g.StudentId == userId)
                .ToListAsync();

            var items = classrooms
                .Select(classroom => new StudentGradeItemVM
                {
                    ClassroomId = classroom.Id,
                    ClassroomName = classroom.NameClass,
                    ClassroomCode = classroom.ClassCode,
                    CourseName = classroom.Course?.Title ?? "N/A",
                    StartDate = classroom.StartDate,
                    EndDate = classroom.EndDate,
                    Grade = grades.FirstOrDefault(g => g.ClassroomId == classroom.Id)
                })
                .OrderByDescending(x => x.StartDate)
                .ToList();

            var model = new StudentGradeIndexVM
            {
                Items = items
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int classroomId)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra học sinh có thuộc lớp này không
            var classroomStudent = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == userId);

            if (classroomStudent == null)
                return Forbid();

            // Lấy thông tin lớp
            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId)
                .Include(c => c.Course)
                .FirstOrDefaultAsync();

            if (classroom == null)
                return NotFound();

            // Lấy bản ghi điểm
            var grade = await _context.ClassroomGrades
                .Where(g => g.ClassroomId == classroomId && g.StudentId == userId)
                .Include(g => g.GradedByInstructor)
                .FirstOrDefaultAsync();

            if (grade == null)
            {
                // Chưa có điểm, hiển thị thông báo
                ViewBag.ClassroomId = classroomId;
                ViewBag.ClassroomName = classroom.NameClass;
                ViewBag.ClassroomCode = classroom.ClassCode;
                ViewBag.CourseName = classroom.Course?.Title;
                ViewBag.HasGrade = false;
                return View("DetailEmpty");
            }

            ViewBag.ClassroomId = classroomId;
            ViewBag.ClassroomName = classroom.NameClass;
            ViewBag.ClassroomCode = classroom.ClassCode;
            ViewBag.CourseName = classroom.Course?.Title;
            ViewBag.GradeDescription = GradeCalculator.GetGradeDescription(grade.GradeLetterClass);
            ViewBag.GradeRange = GradeCalculator.GetGradeRange(grade.GradeLetterClass);
            ViewBag.HasGrade = true;

            return View(grade);
        }

        [HttpGet]
        public async Task<IActionResult> Transcript()
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách lớp và điểm của học sinh
            var classrooms = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == userId)
                .Include(cs => cs.Classroom)
                    .ThenInclude(c => c.Course)
                .Select(cs => cs.Classroom)
                .ToListAsync();

            var grades = await _context.ClassroomGrades
                .Where(g => g.StudentId == userId)
                .ToListAsync();

            var items = classrooms
                .Select(classroom => new StudentGradeItemVM
                {
                    ClassroomId = classroom.Id,
                    ClassroomName = classroom.NameClass,
                    ClassroomCode = classroom.ClassCode,
                    CourseName = classroom.Course?.Title ?? "N/A",
                    StartDate = classroom.StartDate,
                    EndDate = classroom.EndDate,
                    Grade = grades.FirstOrDefault(g => g.ClassroomId == classroom.Id)
                })
                .OrderByDescending(x => x.StartDate)
                .ToList();

            var gradeCount = items.Count(x => x.Grade != null);
            var avgScore = gradeCount > 0
                ? items.Where(x => x.Grade != null).Average(x => x.Grade.FinalScore)
                : 0m;

            ViewBag.TotalCourses = items.Count;
            ViewBag.GradedCourses = gradeCount;
            ViewBag.AverageScore = avgScore.ToString("F2");
            ViewBag.AverageGPA = (avgScore > 0 ? GradeCalculator.CalculateGPA(avgScore) : 0).ToString("F2");

            return View(items);
        }

        // ==================== COMPLETED - Xem khóa học hoàn thành/chưa hoàn thành ====================
        /// <summary>
        /// Xem danh sách khóa học đã hoàn thành (D-A) và chưa hoàn thành (F hoặc không có điểm)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Completed()
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách lớp của học sinh
            var classrooms = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == userId)
                .Include(cs => cs.Classroom)
                    .ThenInclude(c => c.Course)
                .Select(cs => cs.Classroom)
                .ToListAsync();

            // Lấy danh sách điểm của học sinh
            var grades = await _context.ClassroomGrades
                .Where(g => g.StudentId == userId)
                .ToListAsync();

            // Phân chia khóa học thành 2 nhóm
            var allItems = classrooms
                .Select(classroom => new StudentGradeItemVM
                {
                    ClassroomId = classroom.Id,
                    ClassroomName = classroom.NameClass,
                    ClassroomCode = classroom.ClassCode,
                    CourseName = classroom.Course?.Title ?? "N/A",
                    StartDate = classroom.StartDate,
                    EndDate = classroom.EndDate,
                    Grade = grades.FirstOrDefault(g => g.ClassroomId == classroom.Id)
                })
                .OrderByDescending(x => x.StartDate)
                .ToList();

            // Phân loại: Hoàn thành (D-A) vs Chưa hoàn thành (F hoặc null)
            var completedCourses = allItems
                .Where(x => x.Grade != null && x.Grade.GradeLetterClass != GradeLetterEnum.F)
                .ToList();

            var incompleteCourses = allItems
                .Where(x => x.Grade == null || x.Grade.GradeLetterClass == GradeLetterEnum.F)
                .ToList();

            var model = new StudentCourseCompletionVM
            {
                CompletedCourses = completedCourses,
                IncompleteCourses = incompleteCourses,
                TotalCourses = allItems.Count,
                CompletedCount = completedCourses.Count,
                IncompleteCount = incompleteCourses.Count,
                CompletionRate = allItems.Count > 0 
                    ? Math.Round((decimal)completedCourses.Count / allItems.Count * 100, 2)
                    : 0
            };

            return View(model);
        }

    }
}