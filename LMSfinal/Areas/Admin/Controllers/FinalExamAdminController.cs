using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FinalExamAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinalExamAdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _context.FinalExamSchedules
                .Include(f => f.Classroom)
                    .ThenInclude(c => c.Course)
                .OrderByDescending(f => f.ExamDate)
                .ToListAsync();

            // Lấy danh sách lớp để chọn
            ViewBag.Classrooms = new SelectList(await _context.Classrooms
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, Name = c.ClassCode + " - " + c.NameClass })
                .ToListAsync(), "Id", "Name");

            return View(schedules);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRandomExam(FinalExamSchedule model)
        {
            try
            {
                // 1. Lấy danh sách Instructor
                var instructors = await _userManager.GetUsersInRoleAsync("Instructor");

                if (!instructors.Any())
                {
                    TempData["error"] = "Không tìm thấy giảng viên nào trong hệ thống để phân công!";
                    return RedirectToAction(nameof(Index));
                }

                // 2. Gán ngẫu nhiên
                var random = new Random();
                var randomInstructor = instructors[random.Next(instructors.Count)];

                model.ProctorName = !string.IsNullOrEmpty(randomInstructor.FullName)
                                    ? randomInstructor.FullName
                                    : randomInstructor.UserName;

                model.CreatedDate = DateTime.Now;

                _context.FinalExamSchedules.Add(model);
                await _context.SaveChangesAsync();

                TempData["success"] = $"Đã tạo lịch thi và phân công giám thị '{model.ProctorName}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        // ... các hàm cũ giữ nguyên

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _context.FinalExamSchedules.FindAsync(id);
            if (schedule == null) return NotFound();
            return Json(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(FinalExamSchedule model)
        {
            try
            {
                if (model.Id == 0) // Tạo mới + Gán ngẫu nhiên
                {
                    var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
                    if (!instructors.Any())
                    {
                        TempData["error"] = "Không có giảng viên để phân công!";
                        return RedirectToAction(nameof(Index));
                    }

                    var random = new Random();
                    var randomInstructor = instructors[random.Next(instructors.Count)];
                    model.ProctorName = !string.IsNullOrEmpty(randomInstructor.FullName)
                                        ? randomInstructor.FullName
                                        : randomInstructor.UserName;

                    model.CreatedDate = DateTime.Now;
                    _context.FinalExamSchedules.Add(model);
                    TempData["success"] = $"Đã tạo lịch và phân công: {model.ProctorName}";
                }
                else // Cập nhật (Giữ nguyên giám thị cũ hoặc bạn có thể gán lại nếu muốn)
                {
                    var existing = await _context.FinalExamSchedules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (existing != null)
                    {
                        model.ProctorName = existing.ProctorName; // Giữ nguyên người coi thi cũ
                        model.CreatedDate = existing.CreatedDate;
                        _context.FinalExamSchedules.Update(model);
                        TempData["success"] = "Đã cập nhật lịch thi thành công!";
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.FinalExamSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.FinalExamSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}