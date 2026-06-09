using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;

        public FinalExamAdminController(
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

                await SendExamNotificationsAsync(model, randomInstructor);

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
                var isCreated = model.Id == 0;
                ApplicationUser? assignedInstructor = null;

                if (isCreated) // Tạo mới + Gán ngẫu nhiên
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

                    assignedInstructor = randomInstructor;

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

                if (isCreated)
                {
                    await SendExamNotificationsAsync(model, assignedInstructor);
                }

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

        private async Task SendExamNotificationsAsync(FinalExamSchedule schedule, ApplicationUser? proctorUser)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync(c => c.Id == schedule.ClassroomId);

            if (classroom == null)
            {
                return;
            }

            var courseTitle = classroom.Course?.Title ?? "Khóa học";
            var classCode = classroom.ClassCode;
            var examDate = schedule.ExamDate.ToString("dd/MM/yyyy");
            var startTime = schedule.StartTime.ToString(@"hh\:mm");
            var duration = schedule.DurationInMinutes;
            var roomName = schedule.RoomName;
            var proctorName = schedule.ProctorName ?? "Chưa xác định";

            var scheduleUrl = Url.Action("ExamSchedule", "Finaltest", new { area = "Student" }, Request.Scheme)
                ?? "/Student/Finaltest/ExamSchedule";

            if (proctorUser != null)
            {
                var instructorTitle = "Phân công coi thi";
                var instructorMessage =
                    $"Bạn được phân công coi thi môn {courseTitle} (Lớp {classCode}) vào {examDate} lúc {startTime}, phòng {roomName}.";

                await _notificationService.CreateAsync(new Notification
                {
                    RecipientUserId = proctorUser.Id,
                    Title = instructorTitle,
                    Message = instructorMessage,
                    Type = "FinalExamSchedule",
                    EntityType = "FinalExamSchedule",
                    EntityId = schedule.Id,
                    CreatedAt = DateTime.Now
                });
            }

            var studentIds = classroom.ClassroomStudents
                .Select(cs => cs.StudentId)
                .Distinct()
                .ToList();

            if (studentIds.Count > 0)
            {
                var studentTitle = "Lịch thi cuối kỳ";
                var studentMessage =
                    $"Lịch thi môn {courseTitle} (Lớp {classCode}) ngày {examDate} lúc {startTime}, phòng {roomName}. Giảng viên: {proctorName}.";

                await _notificationService.CreateManyAsync(
                    studentIds,
                    studentTitle,
                    studentMessage,
                    "FinalExamSchedule",
                    "FinalExamSchedule",
                    schedule.Id);
            }

            var studentEmails = classroom.ClassroomStudents
                .Select(cs => cs.Student)
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.Email))
                .Select(s => s!.Email!)
                .Distinct()
                .ToList();

            if (studentEmails.Count == 0)
            {
                return;
            }

            var subject = $"Lịch thi cuối kỳ - {courseTitle}";
            var emailBody = BuildStudentExamEmailBody(courseTitle, classCode, examDate, startTime, duration, roomName, proctorName, scheduleUrl);

            foreach (var email in studentEmails)
            {
                await _emailSender.SendEmailAsync(email, subject, emailBody);
            }
        }

        private static string BuildStudentExamEmailBody(
            string courseTitle,
            string classCode,
            string examDate,
            string startTime,
            int duration,
            string roomName,
            string proctorName,
            string scheduleUrl)
        {
            return $@"
<div style='background-color:#f4f6f9;padding:40px 0;font-family:Arial,sans-serif;'>
    <div style='max-width:650px;margin:auto;background:white;border-radius:12px;overflow:hidden;box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
        <div style='background:#2563eb;padding:20px;text-align:center;color:white;'>
            <h2 style='margin:0;'>Thông báo lịch thi cuối kỳ</h2>
        </div>
        <div style='padding:30px;color:#333;'>
            <p>Xin chào,</p>
            <p>Bạn có lịch thi cuối kỳ mới:</p>
            <ul style='line-height:1.8;'>
                <li><strong>Môn học:</strong> {courseTitle}</li>
                <li><strong>Lớp:</strong> {classCode}</li>
                <li><strong>Ngày thi:</strong> {examDate}</li>
                <li><strong>Giờ bắt đầu:</strong> {startTime}</li>
                <li><strong>Thời lượng:</strong> {duration} phút</li>
                <li><strong>Phòng thi:</strong> {roomName}</li>
                <li><strong>Giảng viên:</strong> {proctorName}</li>
            </ul>
            <p>
                Xem chi tiết lịch thi tại:
                <a href='{scheduleUrl}' style='color:#2563eb;font-weight:bold;'>Lịch thi của tôi</a>
            </p>
            <p>Vui lòng có mặt đúng giờ. Chúc bạn thi tốt!</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:24px 0;' />
            <p style='font-size:12px;color:#9ca3af;text-align:center;'>© 2026 LMS System</p>
        </div>
    </div>
</div>";
        }
    }
}