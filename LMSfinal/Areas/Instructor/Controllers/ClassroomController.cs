using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class ClassroomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassroomController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var classrooms = await _context.Classrooms
                .Where(x => x.InstructorId == user.Id)
                .Include(x => x.Course)
                .Include(x => x.ClassroomStudents)
                .ThenInclude(cs => cs.Student)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

            return View(classrooms);
        }


        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var classroom = await _context.Classrooms
                .Where(x => x.Id == id && x.InstructorId == user.Id)
                .Include(x => x.Course)
                .Include(x => x.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .Include(x => x.ClassSchedules)
                    .ThenInclude(cs => cs.TimeSlot)
                .FirstOrDefaultAsync();

            if (classroom == null) { return View("NotFound");  }
            

            return View(classroom);
        }


        public async Task<IActionResult> Students(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var classroom = await _context.Classrooms
                .Where(x => x.Id == id && x.InstructorId == user.Id)
                .Include(x => x.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync();

            if (classroom == null) { return View("NotFound"); }

            return View(classroom);
        }



        // Action 1: Giao diện điểm danh
        public async Task<IActionResult> Attendance(int id, DateTime? date)
        {
            var userId = _userManager.GetUserId(User);

            var classroom = await _context.Classrooms
                .Where(c => c.Id == id && c.InstructorId == userId)
                .Include(c => c.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .Include(c => c.ClassSchedules)
                .FirstOrDefaultAsync();

            if (classroom == null) { return View("NotFound"); }

            DateTime selectedDate = (date ?? DateTime.Today).Date;

            if (selectedDate < classroom.StartDate.Date || selectedDate > classroom.EndDate.Date)
            {
                TempData["Error"] = "Ngày điểm danh không nằm trong thời gian lớp học.";
                ViewBag.Classroom = classroom;
                ViewBag.SelectedDate = selectedDate;
                ViewBag.ExistingIsPresent = new Dictionary<string, bool>();
                ViewBag.ExistingIsLate = new Dictionary<string, bool>();
                ViewBag.ExistingNote = new Dictionary<string, string>();
                return View(new List<ApplicationUser>());
            }

            bool isScheduleDay = classroom.ClassSchedules.Any(s => s.DayOfWeek == selectedDate.DayOfWeek);
            if (!isScheduleDay)
            {
                TempData["Error"] = "Ngày này không có lịch học.";
                ViewBag.Classroom = classroom;
                ViewBag.SelectedDate = selectedDate;
                ViewBag.ExistingIsPresent = new Dictionary<string, bool>();
                ViewBag.ExistingIsLate = new Dictionary<string, bool>();
                ViewBag.ExistingNote = new Dictionary<string, string>();
                return View(new List<ApplicationUser>());
            }

            DateTime dayStart = selectedDate;
            DateTime dayEnd = selectedDate.AddDays(1);

            var existingAttendance = await _context.Attendances
                .Where(a => a.ClassroomId == id && a.AttendanceDate >= dayStart && a.AttendanceDate < dayEnd)
                .Select(a => new { a.StudentId, a.IsPresent, a.IsLate, a.Note })
                .ToListAsync();

            ViewBag.Classroom = classroom;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.ExistingIsPresent = existingAttendance.ToDictionary(x => x.StudentId, x => x.IsPresent);
            ViewBag.ExistingIsLate = existingAttendance.ToDictionary(x => x.StudentId, x => x.IsLate);
            ViewBag.ExistingNote = existingAttendance
                .Where(x => !string.IsNullOrWhiteSpace(x.Note))
                .ToDictionary(x => x.StudentId, x => x.Note ?? string.Empty);

            var students = classroom.ClassroomStudents
                .Where(cs => !cs.IsLocked)
                .Select(cs => cs.Student)
                .ToList();

            return View(students);
        }

        // Action 2: Lưu điểm danh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(int classroomId, DateTime attendanceDate, List<AttendanceSubmitModel> models)
        {
            const int AttendanceNoteMaxLength = 500;

            var userId = _userManager.GetUserId(User);

            var classroom = await _context.Classrooms
                .Where(c => c.Id == classroomId && c.InstructorId == userId)
                .Include(c => c.ClassSchedules)
                .Include(c => c.ClassroomStudents)
                .FirstOrDefaultAsync();

            if (classroom == null) { return View("NotFound"); }

            DateTime normalizedDate = attendanceDate.Date;

            if (normalizedDate < classroom.StartDate.Date || normalizedDate > classroom.EndDate.Date)
            {
                TempData["Error"] = "Ngày điểm danh không nằm trong thời gian lớp học.";
                return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
            }

            bool isScheduleDay = classroom.ClassSchedules.Any(s => s.DayOfWeek == normalizedDate.DayOfWeek);
            if (!isScheduleDay)
            {
                TempData["Error"] = "Ngày này không có lịch học.";
                return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
            }

            if (models == null || !models.Any())
            {
                TempData["Error"] = "Không có dữ liệu điểm danh!";
                return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
            }

            if (models.Any(m => string.IsNullOrWhiteSpace(m.StudentId)))
            {
                TempData["Error"] = "Có sinh viên không hợp lệ trong danh sách điểm danh.";
                return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
            }

            var duplicateIds = models
                .GroupBy(m => m.StudentId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Any())
            {
                TempData["Error"] = "Danh sách điểm danh bị trùng sinh viên.";
                return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
            }

            if (models.Any(m => !string.IsNullOrWhiteSpace(m.Note) && m.Note.Length > AttendanceNoteMaxLength))
            {
                TempData["Error"] = "Ghi chú vượt quá 500 ký tự.";
                return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
            }

            var validStudentIds = classroom.ClassroomStudents
                .Where(cs => !cs.IsLocked)
                .Select(cs => cs.StudentId)
                .ToHashSet();

            var validModels = models
                .Where(m => validStudentIds.Contains(m.StudentId))
                .ToList();

            if (!validModels.Any())
            {
                TempData["Error"] = "Danh sách điểm danh không hợp lệ.";
                return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
            }

            DateTime dayStart = normalizedDate;
            DateTime dayEnd = normalizedDate.AddDays(1);

            var oldData = _context.Attendances
                .Where(a => a.ClassroomId == classroomId && a.AttendanceDate >= dayStart && a.AttendanceDate < dayEnd);
            _context.Attendances.RemoveRange(oldData);

            foreach (var item in validModels)
            {
                _context.Attendances.Add(new Attendance
                {
                    ClassroomId = classroomId,
                    StudentId = item.StudentId,
                    AttendanceDate = normalizedDate,
                    IsPresent = item.IsPresent,
                    IsLate = item.IsPresent && item.IsLate,
                    Note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim()
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật điểm danh ngày {normalizedDate:dd/MM/yyyy}";

            return RedirectToAction("Attendance", new { id = classroomId, date = normalizedDate });
        }

        // Action 3: Xem lịch sử điểm danh
        public async Task<IActionResult> AttendanceHistory(int id, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Attendances
                .Where(a => a.ClassroomId == id)
                .AsQueryable();

            // Lọc theo ngày bắt đầu
            if (startDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate.Date >= startDate.Value.Date);
            }

            // Lọc theo ngày kết thúc
            if (endDate.HasValue)
            {
                query = query.Where(a => a.AttendanceDate.Date <= endDate.Value.Date);
            }

            var history = await query
                .GroupBy(a => a.AttendanceDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    PresentCount = g.Count(x => x.IsPresent),
                    TotalCount = g.Count()
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            // Truyền lại giá trị lọc để hiển thị trên Form
            ViewBag.ClassroomId = id;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            // Vì dữ liệu GroupBy trả về kiểu Anonymous, bạn nên tạo 1 ViewModel 
            // hoặc sử dụng dynamic nếu làm nhanh, ở đây tôi giả định bạn dùng model dynamic ở View
            return View(history);
        }
        public async Task<IActionResult> OverallAttendanceHistory(DateTime? startDate, DateTime? endDate, int? classroomId)
{
    // 1. Lấy thông tin giảng viên hiện tại
    var userId = _userManager.GetUserId(User);

    // 2. Lấy danh sách lớp của giảng viên để làm bộ lọc Dropdown
    var myClassrooms = await _context.Classrooms
        .Where(c => c.InstructorId == userId)
        .ToListAsync();

            // 3. Khởi tạo truy vấn điểm danh dựa trên các lớp của giảng viên này
            var query = _context.Attendances
            .Include(a => a.Classroom) // Phải có Navigation Property ở bước 1 mới dùng được dòng này
            .Where(a => a.Classroom.InstructorId == userId)
            .AsQueryable();

            // 4. Áp dụng các bộ lọc
            if (startDate.HasValue)
        query = query.Where(a => a.AttendanceDate.Date >= startDate.Value.Date);
    
    if (endDate.HasValue)
        query = query.Where(a => a.AttendanceDate.Date <= endDate.Value.Date);
    
    if (classroomId.HasValue)
        query = query.Where(a => a.ClassroomId == classroomId);

    // 5. Nhóm theo Ngày VÀ Lớp học (để tách biệt lịch sử từng buổi của từng lớp)
    var history = await query
        .GroupBy(a => new { a.AttendanceDate.Date, a.ClassroomId, a.Classroom.NameClass, a.Classroom.ClassCode })
        .Select(g => new AttendanceOverallViewModel
        {
            Date = g.Key.Date,
            ClassroomId = g.Key.ClassroomId,
            ClassName = g.Key.NameClass,
            ClassCode = g.Key.ClassCode,
            PresentCount = g.Count(x => x.IsPresent),
            TotalCount = g.Count()
        })
        .OrderByDescending(x => x.Date)
        .ToListAsync();

    ViewBag.Classrooms = myClassrooms;
    ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
    ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
    ViewBag.SelectedClassroom = classroomId;

    return View(history);
}

// ViewModel bổ sung thêm thông tin lớp học
public class AttendanceOverallViewModel
{
    public DateTime Date { get; set; }
    public int ClassroomId { get; set; }
    public string ClassName { get; set; }
    public string ClassCode { get; set; }
    public int PresentCount { get; set; }
    public int TotalCount { get; set; }
}

    }
}
