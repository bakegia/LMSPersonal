using LMSfinal.Data;
using LMSfinal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    public class HomeStudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeStudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Tổng số bài học trong khóa học này
            var totalLessons = await _context.Lessons
                .CountAsync(l => l.Section.ClassroomId == id);

            // 2. Số bài học sinh viên đã hoàn thành (có trong UserProgress)
            var completedLessons = await _context.UserProgresses
                .CountAsync(p => p.UserId == userId && p.Lesson.Section.ClassroomId == id);

            // 3. Tính phần trăm
            double progressPercent = totalLessons > 0
                ? (double)completedLessons / totalLessons * 100
                : 0;
            ViewBag.IsCompleted = await _context.UserProgresses
    .AnyAsync(p => p.UserId == userId && p.LessonId == id);
            ViewBag.ProgressPercent = Math.Round(progressPercent, 0);
            return View();
        }
    }
}
