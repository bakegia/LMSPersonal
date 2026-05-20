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
            var classroom = await _context.Classrooms
                .Include(c => c.ClassroomStudents).ThenInclude(cs => cs.Student)
                .Include(c => c.ClassSchedules)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null) return NotFound();

            // Lấy ngày điểm danh (mặc định là hôm nay nếu không chọn)
            DateTime selectedDate = date ?? DateTime.Today;

            // Lấy danh sách sinh viên đã điểm danh trong ngày này (nếu có)
            var existingAttendance = await _context.Attendances
                .Where(a => a.ClassroomId == id && a.AttendanceDate.Date == selectedDate.Date)
                .ToDictionaryAsync(a => a.StudentId, a => a.IsPresent);

            ViewBag.Classroom = classroom;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.ExistingAttendance = existingAttendance;

            return View(classroom.ClassroomStudents.Select(cs => cs.Student).ToList());
        }

        // Action 2: Lưu điểm danh
        [HttpPost]
        public async Task<IActionResult> SaveAttendance(int classroomId, DateTime attendanceDate, List<AttendanceSubmitModel> models)
        {
            if (models == null || !models.Any())
            {
                TempData["Error"] = "Không có dữ liệu điểm danh!";
                return RedirectToAction("Attendance", new { id = classroomId });
            }

            // 1. Xóa dữ liệu cũ của ngày đó
            var oldData = _context.Attendances
                .Where(a => a.ClassroomId == classroomId && a.AttendanceDate.Date == attendanceDate.Date);
            _context.Attendances.RemoveRange(oldData);

            // 2. Thêm dữ liệu mới từ danh sách models
            foreach (var item in models)
            {
                _context.Attendances.Add(new Attendance
                {
                    ClassroomId = classroomId,
                    StudentId = item.StudentId,
                    AttendanceDate = attendanceDate,
                    IsPresent = item.IsPresent // Ở đây IsPresent sẽ nhận giá trị true nếu checkbox được tích
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật điểm danh ngày {attendanceDate:dd/MM/yyyy}";

            return RedirectToAction("Attendance", new { id = classroomId, date = attendanceDate });
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
