using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.Enums;
using LMSfinal.Models.Utilities;
using LMSfinal.Models.ViewModels;
using LMSfinal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;

        public ScoreController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _emailSender = emailSender;
        }

        // ==================== INDEX - Danh sách lớp để ghi điểm ====================
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
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int classroomId)
        {
            var userId = _userManager.GetUserId(User);

            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId && c.InstructorId == userId)
                .Include(c => c.Course)
                .FirstOrDefaultAsync();

            if (classroom == null)
                return Forbid();

            var grades = await _context.ClassroomGrades
                .Where(g => g.ClassroomId == classroomId)
                .Include(g => g.Student)
                .OrderBy(g => g.Student!.FullName)
                .ToListAsync();

            var studentIds = grades.Select(g => g.StudentId).Distinct().ToList();

            var profileLookup = await _context.UserProfiles
                .Where(up => studentIds.Contains(up.UserId))
                .ToDictionaryAsync(up => up.UserId, up => up);

            const string sep = ";";
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("sep=;");
            csv.AppendLine(string.Join(sep,
                "STT", "MSSV", "Tên Sinh Viên", "Email", "Điểm Quá Trình", "Điểm Giữa Kỳ",
                "Điểm Thi", "Điểm Tổng", "GPA", "Hạng Điểm", "Ghi Chú"));

            int index = 1;
            foreach (var grade in grades)
            {
                profileLookup.TryGetValue(grade.StudentId, out var profile);

                var mssv = profile?.Mssv.ToString() ?? string.Empty;
                var studentName = grade.Student?.FullName ?? profile?.Fullname ?? "N/A";
                var studentEmail = grade.Student?.Email ?? profile?.Email ?? "N/A";

                var processScore = FormatNumber(grade.ProcessScore);
                var midtermScore = FormatNumber(grade.MidtermScore);
                var finalExamScore = FormatNumber(grade.FinalExamScore);
                var finalScore = FormatNumber(grade.FinalScore);
                var gpa = FormatNumber(grade.GPA);

                var row = string.Join(sep,
                    CsvValue(index.ToString()),
                    CsvValue(mssv),
                    CsvValue(studentName),
                    CsvValue(studentEmail),
                    CsvValue(processScore),
                    CsvValue(midtermScore),
                    CsvValue(finalExamScore),
                    CsvValue(finalScore),
                    CsvValue(gpa),
                    CsvValue(grade.GradeLetterClass.ToString()),
                    CsvValue(grade.Comments ?? string.Empty));

                csv.AppendLine(row);
                index++;
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString()))
                .ToArray();

            return File(bytes, "text/csv", $"Scores_{classroom.ClassCode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private static string CsvValue(string value)
        {
            if (value == null)
                return "\"\"";

            var cleaned = value.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ").Trim();
            return $"\"{cleaned}\"";
        }

        private static string FormatNumber(decimal? value)
        {
            return value.HasValue
                ? value.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string FormatNumber(decimal value)
        {
            return value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string BuildGradeMessage(ClassroomGrade grade, Classroom classroom)
        {
            return $"Lớp: {classroom.NameClass} | Môn: {classroom.Course?.Title ?? "N/A"} | " +
                   $"Điểm tổng: {grade.FinalScore:F2} | GPA: {grade.GPA:F2} | Hạng: {grade.GradeLetterClass}.";
        }

        private static string BuildGradeEmail(ClassroomGrade grade, Classroom classroom)
        {
            return $@"
                        <h3>Thông báo điểm</h3>
                        <p><strong>Lớp:</strong> {classroom.NameClass}</p>
                        <p><strong>Môn:</strong> {classroom.Course?.Title ?? "N/A"}</p>
                        <p><strong>Điểm tổng:</strong> {grade.FinalScore:F2}</p>
                        <p><strong>GPA:</strong> {grade.GPA:F2}</p>
                        <p><strong>Hạng điểm:</strong> {grade.GradeLetterClass}</p>
                        <p><strong>Nhận xét:</strong> {grade.Comments ?? "Không có"}</p>";
        }
        // ==================== SEND GRADE NOTIFICATION - SINGLE ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGradeNotification(int classroomId, string studentId, bool sendEmail)
        {
            var userId = _userManager.GetUserId(User);

            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.InstructorId == userId);

            if (classroom == null)
                return Forbid();

            var grade = await _context.ClassroomGrades
                .Include(g => g.Student)
                .FirstOrDefaultAsync(g => g.ClassroomId == classroomId && g.StudentId == studentId);

            if (grade?.Student == null)
            {
                TempData["error"] = "Không tìm thấy điểm hoặc học sinh.";
                return RedirectToAction(nameof(GradeList), new { classroomId });
            }

            var title = "Thông báo điểm";
            var message = BuildGradeMessage(grade, classroom);

            await _notificationService.CreateAsync(new Notification
            {
                RecipientUserId = grade.StudentId,
                Title = title,
                Message = message,
                Type = "GradeUpdated",
                EntityType = "ClassroomGrade",
                EntityId = grade.Id,
                CreatedAt = DateTime.Now
            });

            if (sendEmail && !string.IsNullOrWhiteSpace(grade.Student.Email))
            {
                var emailBody = BuildGradeEmail(grade, classroom);
                await _emailSender.SendEmailAsync(grade.Student.Email, title, emailBody);
            }

            TempData["success"] = $"Đã gửi thông báo điểm cho {grade.Student.FullName}";
            return RedirectToAction(nameof(GradeList), new { classroomId });
        }

        // ==================== SEND GRADE NOTIFICATION - ALL ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAllGradeNotifications(int classroomId, bool sendEmail)
        {
            var userId = _userManager.GetUserId(User);

            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.InstructorId == userId);

            if (classroom == null)
                return Forbid();

            var grades = await _context.ClassroomGrades
                .Include(g => g.Student)
                .Where(g => g.ClassroomId == classroomId)
                .ToListAsync();

            if (!grades.Any())
            {
                TempData["error"] = "Chưa có dữ liệu điểm để gửi.";
                return RedirectToAction(nameof(GradeList), new { classroomId });
            }

            var notifications = new List<Notification>();

            foreach (var grade in grades)
            {
                if (grade.Student == null)
                    continue;

                notifications.Add(new Notification
                {
                    RecipientUserId = grade.StudentId,
                    Title = "Thông báo điểm",
                    Message = BuildGradeMessage(grade, classroom),
                    Type = "GradeUpdated",
                    EntityType = "ClassroomGrade",
                    EntityId = grade.Id,
                    CreatedAt = DateTime.Now
                });

                if (sendEmail && !string.IsNullOrWhiteSpace(grade.Student.Email))
                {
                    var emailBody = BuildGradeEmail(grade, classroom);
                    await _emailSender.SendEmailAsync(grade.Student.Email, "Thông báo điểm", emailBody);
                }
            }

            if (notifications.Count > 0)
            {
                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }

            TempData["success"] = $"Đã gửi thông báo điểm cho {notifications.Count} học sinh";
            return RedirectToAction(nameof(GradeList), new { classroomId });
        }

        // ==================== SEND GRADE NOTIFICATION - SELECTED ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSelectedGradeNotifications(int classroomId, List<string> studentIds, bool sendEmail)
        {
            var userId = _userManager.GetUserId(User);

            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.InstructorId == userId);

            if (classroom == null)
                return Forbid();

            if (studentIds == null || studentIds.Count == 0)
            {
                TempData["error"] = "Vui lòng chọn ít nhất một học sinh.";
                return RedirectToAction(nameof(GradeList), new { classroomId });
            }

            var grades = await _context.ClassroomGrades
                .Include(g => g.Student)
                .Where(g => g.ClassroomId == classroomId && studentIds.Contains(g.StudentId))
                .ToListAsync();

            if (!grades.Any())
            {
                TempData["error"] = "Không tìm thấy điểm của học sinh đã chọn.";
                return RedirectToAction(nameof(GradeList), new { classroomId });
            }

            var notifications = new List<Notification>();

            foreach (var grade in grades)
            {
                if (grade.Student == null)
                    continue;

                notifications.Add(new Notification
                {
                    RecipientUserId = grade.StudentId,
                    Title = "Thông báo điểm",
                    Message = BuildGradeMessage(grade, classroom),
                    Type = "GradeUpdated",
                    EntityType = "ClassroomGrade",
                    EntityId = grade.Id,
                    CreatedAt = DateTime.Now
                });

                if (sendEmail && !string.IsNullOrWhiteSpace(grade.Student.Email))
                {
                    var emailBody = BuildGradeEmail(grade, classroom);
                    await _emailSender.SendEmailAsync(grade.Student.Email, "Thông báo điểm", emailBody);
                }
            }

            if (notifications.Count > 0)
            {
                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }

            TempData["success"] = $"Đã gửi thông báo điểm cho {notifications.Count} học sinh";
            return RedirectToAction(nameof(GradeList), new { classroomId });
        }
    }
}