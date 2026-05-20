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

namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class ScoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScoreController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== INDEX - Danh sách lớp để ghi điểm ====================
        /// <summary>
        /// Hiển thị danh sách lớp của giáo viên để chọn ghi điểm
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var classrooms = await _context.Classrooms
                .Where(c => c.InstructorId == userId && c.IsActive)
                .Include(c => c.Course)
                .Include(c => c.ClassroomStudents)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return View(classrooms);
        }

        // ==================== GRADELIST - Danh sách học sinh để ghi điểm ====================
        /// <summary>
        /// Hiển thị danh sách học sinh trong lớp để ghi điểm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GradeList(int classroomId)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra quyền truy cập - chỉ giáo viên của lớp mới được phép
            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId && c.InstructorId == userId)
                .Include(c => c.Course)
                .Include(c => c.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync();

            if (classroom == null)
                return Forbid();

            // Lấy danh sách điểm của các học sinh trong lớp
            var grades = await _context.ClassroomGrades
                .Where(g => g.ClassroomId == classroomId)
                .Include(g => g.Student)
                .ToListAsync();

            // Tạo danh sách học sinh với điểm của họ
            var gradeList = classroom.ClassroomStudents
                .Select(cs => new StudentGradeItemViewModel
                {
                    StudentId = cs.StudentId,
                    StudentName = cs.Student?.FullName ?? "N/A",
                    StudentEmail = cs.Student?.Email ?? "N/A",
                    GradeId = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.Id ?? 0,
                    ProcessScore = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.ProcessScore,
                    MidtermScore = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.MidtermScore,
                    FinalExamScore = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.FinalExamScore,
                    FinalScore = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.FinalScore ?? 0,
                    GPA = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.GPA ?? 0,
                    GradeLetterClass = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.GradeLetterClass ?? GradeLetterEnum.F,
                    Comments = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.Comments,
                    UpdatedDate = grades.FirstOrDefault(g => g.StudentId == cs.StudentId)?.UpdatedDate
                })
                .OrderBy(s => s.StudentName)
                .ToList();

            var model = new ClassroomGradeListViewModel
            {
                ClassroomId = classroom.Id,
                ClassroomCode = classroom.ClassCode,
                ClassroomName = classroom.NameClass,
                CourseName = classroom.Course?.Title ?? "N/A",
                StudentGrades = gradeList
            };

            return View(model);
        }

        // ==================== GRADEINPUT - Form nhập/sửa điểm ====================
        /// <summary>
        /// Hiển thị form nhập/sửa điểm cho một học sinh
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GradeInput(int classroomId, string studentId)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra quyền truy cập
            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId && c.InstructorId == userId)
                .Include(c => c.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync();

            if (classroom == null)
                return Forbid();

            // Kiểm tra học sinh có trong lớp không
            var classroomStudent = classroom.ClassroomStudents
                .FirstOrDefault(cs => cs.StudentId == studentId);

            if (classroomStudent?.Student == null)
                return NotFound();

            var student = classroomStudent.Student;

            // Tìm bản ghi điểm nếu có
            var existingGrade = await _context.ClassroomGrades
                .FirstOrDefaultAsync(g => g.ClassroomId == classroomId && g.StudentId == studentId);

            var model = new GradeInputViewModel
            {
                ClassroomId = classroomId,
                StudentId = studentId,
                GradeId = existingGrade?.Id ?? 0,
                StudentName = student.FullName,
                StudentEmail = student.Email,
                ClassroomName = classroom.NameClass,
                ProcessScore = existingGrade?.ProcessScore,
                MidtermScore = existingGrade?.MidtermScore,
                FinalExamScore = existingGrade?.FinalExamScore,
                Comments = existingGrade?.Comments,
                FinalScore = existingGrade?.FinalScore ?? 0,
                GPA = existingGrade?.GPA ?? 0,
                GradeLetterClass = existingGrade?.GradeLetterClass ?? GradeLetterEnum.F
            };

            return View(model);
        }

        /// <summary>
        /// Lưu/cập nhật điểm cho học sinh
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeInput(GradeInputViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);

            // Kiểm tra quyền truy cập
            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == model.ClassroomId && c.InstructorId == userId);

            if (classroom == null)
                return Forbid();

            // Kiểm tra học sinh trong lớp
            var classroomStudent = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == model.ClassroomId && cs.StudentId == model.StudentId);

            if (classroomStudent == null)
                return NotFound();

            try
            {
                ClassroomGrade grade;

                if (model.GradeId > 0)
                {
                    // Cập nhật bản ghi cũ
                    grade = await _context.ClassroomGrades
                        .FirstOrDefaultAsync(g => g.Id == model.GradeId);

                    if (grade == null)
                        return NotFound();
                }
                else
                {
                    // Tạo bản ghi mới
                    grade = new ClassroomGrade
                    {
                        ClassroomId = model.ClassroomId,
                        StudentId = model.StudentId,
                        CreatedDate = DateTime.Now
                    };
                    _context.ClassroomGrades.Add(grade);
                }

                // Cập nhật giá trị điểm
                grade.ProcessScore = model.ProcessScore;
                grade.MidtermScore = model.MidtermScore;
                grade.FinalExamScore = model.FinalExamScore;
                grade.Comments = model.Comments;
                grade.GradedByInstructorId = userId;
                grade.UpdatedDate = DateTime.Now;

                // Tính toán điểm tổng, GPA và hạng điểm
                grade.FinalScore = GradeCalculator.CalculateFinalScore(
                    model.ProcessScore, model.MidtermScore, model.FinalExamScore);
                grade.GPA = GradeCalculator.CalculateGPA(grade.FinalScore);
                grade.GradeLetterClass = GradeCalculator.CalculateGradeLetter(grade.FinalScore);

                await _context.SaveChangesAsync();

                TempData["success"] = $"Cập nhật điểm cho {classroomStudent.Student?.FullName} thành công";
                return RedirectToAction(nameof(GradeList), new { classroomId = model.ClassroomId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                return View(model);
            }
        }

        // ==================== GRADEDETAIL - Xem chi tiết điểm ====================
        /// <summary>
        /// Xem chi tiết điểm của một học sinh
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GradeDetail(int classroomId, string studentId)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra quyền truy cập
            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId && c.InstructorId == userId)
                .Include(c => c.Course)
                .FirstOrDefaultAsync();

            if (classroom == null)
                return Forbid();

            // Tìm bản ghi điểm
            var grade = await _context.ClassroomGrades
                .Where(g => g.ClassroomId == classroomId && g.StudentId == studentId)
                .Include(g => g.Student)
                .Include(g => g.GradedByInstructor)
                .FirstOrDefaultAsync();

            if (grade == null)
                return NotFound();

            var model = new GradeDetailViewModel
            {
                GradeId = grade.Id,
                ClassroomId = classroomId,
                StudentId = studentId,
                StudentName = grade.Student?.FullName ?? "N/A",
                StudentEmail = grade.Student?.Email ?? "N/A",
                ClassroomName = classroom.NameClass,
                CourseName = classroom.Course?.Title ?? "N/A",
                ProcessScore = grade.ProcessScore,
                MidtermScore = grade.MidtermScore,
                FinalExamScore = grade.FinalExamScore,
                FinalScore = grade.FinalScore,
                GPA = grade.GPA,
                GradeLetterClass = grade.GradeLetterClass,
                GradeDescription = GradeCalculator.GetGradeDescription(grade.GradeLetterClass),
                Comments = grade.Comments,
                GradedByInstructorName = grade.GradedByInstructor?.FullName,
                CreatedDate = grade.CreatedDate,
                UpdatedDate = grade.UpdatedDate
            };

            return View(model);
        }

        // ==================== DELETE - Xóa điểm ====================
        /// <summary>
        /// Xóa bản ghi điểm của học sinh
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGrade(int gradeId, int classroomId)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra quyền truy cập
            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.InstructorId == userId);

            if (classroom == null)
                return Forbid();

            // Tìm và xóa bản ghi điểm
            var grade = await _context.ClassroomGrades
                .FirstOrDefaultAsync(g => g.Id == gradeId && g.ClassroomId == classroomId);

            if (grade == null)
                return NotFound();

            try
            {
                _context.ClassroomGrades.Remove(grade);
                await _context.SaveChangesAsync();

                TempData["success"] = "Xóa điểm thành công";
                return RedirectToAction(nameof(GradeList), new { classroomId });
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xóa điểm: {ex.Message}";
                return RedirectToAction(nameof(GradeList), new { classroomId });
            }
        }

        // ==================== BULK IMPORT - Nhập điểm hàng loạt ====================
        /// <summary>
        /// Hiển thị form nhập điểm hàng loạt (CSV/Excel)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BulkImport(int classroomId)
        {
            var userId = _userManager.GetUserId(User);

            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId && c.InstructorId == userId)
                .Include(c => c.Course)
                .FirstOrDefaultAsync();

            if (classroom == null)
                return Forbid();

            ViewBag.ClassroomId = classroomId;
            ViewBag.ClassroomName = classroom.NameClass;

            return View();
        }

        // ==================== EXPORT - Xuất danh sách điểm ====================
        /// <summary>
        /// Xuất danh sách điểm thành CSV/Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int classroomId)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra quyền truy cập
            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId && c.InstructorId == userId)
                .Include(c => c.Course)
                .FirstOrDefaultAsync();

            if (classroom == null)
                return Forbid();

            // Lấy danh sách điểm
            var grades = await _context.ClassroomGrades
                .Where(g => g.ClassroomId == classroomId)
                .Include(g => g.Student)
                .OrderBy(g => g.Student!.FullName)
                .ToListAsync();

            // Tạo CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("STT,Mã Số Sinh Viên,Tên Sinh Viên,Email,Điểm Quá Trình,Điểm Giữa Kỳ,Điểm Thi,Điểm Tổng,GPA,Hạng Điểm,Ghi Chú");

            int index = 1;
            foreach (var grade in grades)
            {
                csv.AppendLine($"{index},\"{grade.StudentId}\",\"{grade.Student?.FullName}\",\"{grade.Student?.Email}\"," +
                    $"{grade.ProcessScore:F2},{grade.MidtermScore:F2},{grade.FinalExamScore:F2}," +
                    $"{grade.FinalScore:F2},{grade.GPA:F2},{grade.GradeLetterClass},\"{grade.Comments}\"");
                index++;
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                $"Scores_{classroom.ClassCode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}