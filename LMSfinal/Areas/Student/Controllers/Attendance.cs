using LMSfinal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    public class Attendance : Controller
    {
        
        private readonly ApplicationDbContext _context;

        public Attendance(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> MyAttendanceHistory(int? classroomId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId)) return Challenge();

            // 1. Lấy danh sách các lớp học sinh này tham gia để làm bộ lọc
            var myClassrooms = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == studentId)
                .Select(cs => cs.Classroom)
                .ToListAsync();

            // 2. Truy vấn lịch sử điểm danh
            var query = _context.Attendances
                .Include(a => a.Classroom)
                .Where(a => a.StudentId == studentId)
                .AsQueryable();

            if (classroomId.HasValue)
            {
                query = query.Where(a => a.ClassroomId == classroomId);
            }

            var history = await query
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            // 3. Tính toán thống kê tổng quát
            ViewBag.TotalSessions = history.Count;
            ViewBag.PresentCount = history.Count(a => a.IsPresent);
            ViewBag.AbsentCount = history.Count(a => !a.IsPresent);
            ViewBag.Classrooms = myClassrooms;
            ViewBag.SelectedClassroom = classroomId;

            return View(history);
        }
    }
}
