using LMSfinal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    public class FinaltestController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FinaltestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        // Thêm vào Index hoặc một Action mới ví dụ ExamSchedule()
        public async Task<IActionResult> ExamSchedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy ID các lớp học sinh viên đã tham gia
            var enrolledClassIds = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == userId)
                .Select(cs => cs.ClassroomId)
                .ToListAsync();

            // Lấy lịch thi của các lớp đó
            var examSchedules = await _context.FinalExamSchedules
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Course)
                .Where(e => enrolledClassIds.Contains(e.ClassroomId))
                .OrderBy(e => e.ExamDate)
                .ToListAsync();

            return View(examSchedules);
        }
    }

}
