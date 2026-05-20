using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class ScheduleTeach : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleTeach(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(int? classroomId, DayOfWeek? dayOfWeek)
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách lớp để nạp vào Dropdown lọc
            ViewBag.Classrooms = await _context.Set<Classroom>()
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            var query = _context.Set<ClassSchedule>()
                .Include(cs => cs.TimeSlot)
                .Include(cs => cs.Classroom)
                    .ThenInclude(c => c.Course)
                .Where(cs => cs.Classroom.InstructorId == userId)
                .AsQueryable();

            // Thực hiện lọc nếu có tham số
            if (classroomId.HasValue)
            {
                query = query.Where(cs => cs.ClassroomId == classroomId);
            }
            if (dayOfWeek.HasValue)
            {
                query = query.Where(cs => cs.DayOfWeek == dayOfWeek);
            }

            var result = await query.OrderBy(cs => cs.DayOfWeek)
                                    .ThenBy(cs => cs.TimeSlot.StartTime)
                                    .ToListAsync();

            return View(result);
        }
        public async Task<IActionResult> WeeklyGrid(DateTime? date)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Lấy danh sách các lớp học của giảng viên để xác định khoảng thời gian
            var instructorClassrooms = await _context.Set<Classroom>()
                .Where(c => c.InstructorId == userId && c.IsActive)
                .ToListAsync();

            if (!instructorClassrooms.Any())
            {
                return View(new WeeklyScheduleViewModel { /* Trả về view trống */ });
            }

            // Tìm ngày bắt đầu sớm nhất và kết thúc muộn nhất
            DateTime minStart = instructorClassrooms.Min(c => c.StartDate);
            DateTime maxEnd = instructorClassrooms.Max(c => c.EndDate);

            // Đưa minStart về Thứ 2 của tuần đó
            int diffMin = (7 + (minStart.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime firstMonday = minStart.AddDays(-1 * diffMin).Date;

            // 2. Tạo danh sách tuần từ firstMonday cho đến khi vượt quá maxEnd
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

            // 3. Xác định tuần đang hiển thị (mặc định là tuần hiện tại hoặc tuần đầu tiên nếu chưa đến kỳ)
            DateTime referenceDate = date ?? (DateTime.Now > minStart && DateTime.Now < maxEnd ? DateTime.Now : minStart);
            int diff = (7 + (referenceDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = referenceDate.AddDays(-1 * diff).Date;
            var daysInWeek = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

            // 4. Lấy dữ liệu lịch dạy
            var timeSlots = await _context.Set<TimeSlot>().OrderBy(t => t.StartTime).ToListAsync();
            var schedules = await _context.Set<ClassSchedule>()
                .Include(cs => cs.Classroom).ThenInclude(c => c.Instructor)
                .Include(cs => cs.TimeSlot)
                .Where(cs => cs.Classroom.InstructorId == userId)
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
