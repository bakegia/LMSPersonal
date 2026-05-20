using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels;
using LMSfinal.Models.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> MySchedule(int? classroomId, DayOfWeek? dayOfWeek)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Lấy danh sách ID các lớp mà học sinh này tham gia từ bảng ClassroomStudent
            var enrolledClassIds = await _context.Set<ClassroomStudent>()
                .Where(cs => cs.StudentId == userId)
                .Select(cs => cs.ClassroomId)
                .ToListAsync();

            // 2. Nạp danh sách lớp vào ViewBag để hiển thị Dropdown lọc
            ViewBag.Classrooms = await _context.Set<Classroom>()
                .Where(c => enrolledClassIds.Contains(c.Id))
                .ToListAsync();

            // 3. Truy vấn lịch học dựa trên các ID lớp đã tìm được
            var query = _context.Set<ClassSchedule>()
                .Include(s => s.TimeSlot)
                .Include(s => s.Classroom)
                    .ThenInclude(c => c.Course)
                .Include(s => s.Classroom)
                    .ThenInclude(c => c.Instructor) // Lấy thông tin giảng viên
                .Where(s => enrolledClassIds.Contains(s.ClassroomId))
                .AsQueryable();

            // Lọc theo ClassroomId nếu người dùng chọn trên giao diện
            if (classroomId.HasValue)
            {
                query = query.Where(s => s.ClassroomId == classroomId);
            }

            // Lọc theo Thứ nếu người dùng chọn
            if (dayOfWeek.HasValue)
            {
                query = query.Where(s => s.DayOfWeek == dayOfWeek);
            }

            var result = await query.OrderBy(s => s.DayOfWeek)
                                    .ThenBy(s => s.TimeSlot.StartTime)
                                    .ToListAsync();

            return View(result);
        }

        public async Task<IActionResult> WeeklyGrid(DateTime? date)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Lấy tất cả thông tin lớp học của học sinh này
            var studentClasses = await _context.Set<ClassroomStudent>()
                .Include(cs => cs.Classroom)
                .Where(cs => cs.StudentId == userId && cs.Classroom.IsActive)
                .Select(cs => cs.Classroom)
                .ToListAsync();

            if (!studentClasses.Any())
            {
                // Trả về View trống nếu học sinh chưa tham gia lớp nào
                return View(new WeeklyScheduleViewModel
                {
                    TimeSlots = new List<TimeSlot>(),
                    Schedules = new List<ClassSchedule>()
                });
            }

            // --- TÍNH TOÁN KHOẢNG THỜI GIAN HỌC ---
            DateTime minStart = studentClasses.Min(c => c.StartDate);
            DateTime maxEnd = studentClasses.Max(c => c.EndDate);

            // Đưa về Thứ 2 đầu tiên của kỳ học
            int diffMin = (7 + (minStart.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime firstMonday = minStart.AddDays(-1 * diffMin).Date;

            // --- TẠO DANH SÁCH CÁC TUẦN ĐỂ CHỌN (DROPDOWN) ---
            var weeks = new List<WeekItem>();
            DateTime currentWeekStart = firstMonday;
            int weekCounter = 1;

            while (currentWeekStart <= maxEnd)
            {
                DateTime currentWeekEnd = currentWeekStart.AddDays(6);
                weeks.Add(new WeekItem
                {
                    WeekNumber = weekCounter++,
                    StartDate = currentWeekStart,
                    EndDate = currentWeekEnd,
                    IsSelected = (date.HasValue && date.Value.Date >= currentWeekStart && date.Value.Date <= currentWeekEnd)
                                 || (!date.HasValue && DateTime.Now.Date >= currentWeekStart && DateTime.Now.Date <= currentWeekEnd)
                });
                currentWeekStart = currentWeekStart.AddDays(7);
            }

            // --- XÁC ĐỊNH TUẦN ĐANG HIỂN THỊ ---
            DateTime referenceDate = date ?? (DateTime.Now >= minStart && DateTime.Now <= maxEnd ? DateTime.Now : minStart);
            int diff = (7 + (referenceDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = referenceDate.AddDays(-1 * diff).Date;
            var daysInWeek = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

            // --- LẤY DỮ LIỆU LỊCH HỌC VÀ CA HỌC ---
            var classIds = studentClasses.Select(c => c.Id).ToList();
            var timeSlots = await _context.Set<TimeSlot>().OrderBy(t => t.StartTime).ToListAsync();

            var schedules = await _context.Set<ClassSchedule>()
                .Include(s => s.Classroom).ThenInclude(c => c.Course)
                .Include(s => s.Classroom).ThenInclude(c => c.Instructor)
                .Include(s => s.TimeSlot)
                .Where(s => classIds.Contains(s.ClassroomId))
                .ToListAsync();

            ViewBag.Weeks = weeks;

            var viewModel = new WeeklyScheduleViewModel
            {
                TimeSlots = timeSlots,
                Schedules = schedules,
                WeekStart = startOfWeek,
                DaysInWeek = daysInWeek
            };

            return View(viewModel);
        }
    }
}
