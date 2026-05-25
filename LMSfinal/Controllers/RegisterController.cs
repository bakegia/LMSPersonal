using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Controllers
{
    [Authorize]
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IScheduleConflictService _scheduleConflictService;

        public RegisterController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IScheduleConflictService scheduleConflictService)
        {
            _context = context;
            _userManager = userManager;
            _scheduleConflictService = scheduleConflictService;
        }

        // ==================== STUDENT REGISTRATION ====================
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> AvailableClassroomsForStudent()
        {
            var currentDate = DateTime.Now;

            // Lấy danh sách lớp mở đăng ký
            var classrooms = await _context.Classrooms
                .Where(c => c.IsOpenForRegistration &&
                            c.InstructorId != null && // Đã có giáo viên
                            c.ClassroomStudents.Count() < c.MaxCapacity && // Đổi CurrentEnrollment thành đếm trực tiếp ở đây
                            (!c.RegistrationDeadline.HasValue || c.RegistrationDeadline >= currentDate) && // Hạn chưa hết (đã bọc ngoặc)
                            c.IsActive)
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.TimeSlot)
                .Include(c => c.ClassroomStudents) // Giữ nguyên để View có thể dùng nếu cần
                .OrderBy(c => c.StartDate)
                .ToListAsync();

            return View(classrooms);
        }

        /// <summary>
        /// Học sinh đăng ký vào lớp
        /// </summary>
        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> StudentRegister(int classroomId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentDate = DateTime.Now;

            // ===== KIỂM TRA CƠ BẢN =====
            var classroom = await _context.Classrooms
                .Include(c => c.ClassroomStudents)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.TimeSlot)
                .FirstOrDefaultAsync(c => c.Id == classroomId);

            if (classroom == null)
            {
                TempData["error"] = "❌ Lớp học không tồn tại.";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            // Kiểm tra lớp có mở đăng ký không?
            if (!classroom.IsOpenForRegistration)
            {
                TempData["error"] = "❌ Lớp này không mở đăng ký.";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            // Kiểm tra lớp đã có giáo viên chưa?
            if (string.IsNullOrEmpty(classroom.InstructorId))
            {
                TempData["error"] = "❌ Lớp này chưa có giáo viên. Vui lòng chọn lớp khác.";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            // Kiểm tra lớp đã đủ sức chứa?
            if (classroom.CurrentEnrollment >= classroom.MaxCapacity)
            {
                TempData["error"] = $"❌ Lớp này đã đủ sức chứa ({classroom.CurrentEnrollment}/{classroom.MaxCapacity}).";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            // Kiểm tra hạn đăng ký có hết không?
            if (classroom.RegistrationDeadline.HasValue && currentDate > classroom.RegistrationDeadline)
            {
                TempData["error"] = "❌ Hạn đăng ký lớp này đã hết.";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            // Kiểm tra học sinh đã đăng ký lớp này rồi không?
            var existingEnrollment = classroom.ClassroomStudents
                .FirstOrDefault(cs => cs.StudentId == currentUserId);

            if (existingEnrollment != null)
            {
                TempData["error"] = "❌ Bạn đã đăng ký lớp này rồi.";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            // ===== KIỂM TRA TRÙNG LỊCH =====
            var (hasConflict, conflictMessage) =
                await _scheduleConflictService.CheckStudentScheduleConflict(currentUserId, classroomId);

            if (hasConflict)
            {
                TempData["error"] = $"❌ {conflictMessage}";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            // ===== THÊM VÀO LỚP =====
            try
            {
                var classroomStudent = new ClassroomStudent
                {
                    ClassroomId = classroomId,
                    StudentId = currentUserId
                };

                _context.ClassroomStudents.Add(classroomStudent);
                await _context.SaveChangesAsync();

                TempData["success"] = $"✅ Bạn đã đăng ký lớp '{classroom.NameClass}' thành công!";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }
            catch (Exception ex)
            {
                TempData["error"] = $"❌ Lỗi: {ex.Message}";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }
        }

        // ==================== INSTRUCTOR REGISTRATION ====================

        /// <summary>
        /// Danh sách lớp cần giáo viên
        /// </summary>
        [Authorize(Roles = "Instructor")]
        [HttpGet]
        public async Task<IActionResult> AvailableClassroomsForInstructor()
        {
            var currentDate = DateTime.Now;

            // Lấy danh sách lớp chưa có giáo viên
            var classrooms = await _context.Classrooms
                .Where(c => c.IsOpenForRegistration &&
                           c.InstructorId == null && // Chưa có giáo viên
                           (!c.RegistrationDeadline.HasValue || c.RegistrationDeadline >= currentDate) && // Hạn chưa hết
                           c.IsActive)
                .Include(c => c.Course)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.TimeSlot)
                .OrderBy(c => c.StartDate)
                .ToListAsync();

            return View(classrooms);
        }

        /// <summary>
        /// Giáo viên đăng ký dạy lớp
        /// </summary>
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<IActionResult> InstructorRegister(int classroomId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentDate = DateTime.Now;

            // ===== KIỂM TRA CƠ BẢN =====
            var classroom = await _context.Classrooms
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.TimeSlot)
                .FirstOrDefaultAsync(c => c.Id == classroomId);

            if (classroom == null)
            {
                TempData["error"] = "❌ Lớp học không tồn tại.";
                return RedirectToAction(nameof(AvailableClassroomsForInstructor));
            }

            // Kiểm tra lớp có mở đăng ký không?
            if (!classroom.IsOpenForRegistration)
            {
                TempData["error"] = "❌ Lớp này không mở đăng ký.";
                return RedirectToAction(nameof(AvailableClassroomsForInstructor));
            }

            // Kiểm tra lớp đã có giáo viên chưa?
            if (!string.IsNullOrEmpty(classroom.InstructorId))
            {
                TempData["error"] = "❌ Lớp này đã có giáo viên dạy rồi.";
                return RedirectToAction(nameof(AvailableClassroomsForInstructor));
            }

            // Kiểm tra hạn đăng ký có hết không?
            if (classroom.RegistrationDeadline.HasValue && currentDate > classroom.RegistrationDeadline)
            {
                TempData["error"] = "❌ Hạn đăng ký lớp này đã hết.";
                return RedirectToAction(nameof(AvailableClassroomsForInstructor));
            }

            // ===== KIỂM TRA TRÙNG LỊCH GIÁO VIÊN =====
            var (hasConflict, conflictMessage) =
                await _scheduleConflictService.CheckInstructorScheduleConflict(currentUserId, classroomId);

            if (hasConflict)
            {
                TempData["error"] = $"❌ {conflictMessage}";
                return RedirectToAction(nameof(AvailableClassroomsForInstructor));
            }

            // ===== ASSIGN GIÁO VIÊN =====
            try
            {
                classroom.InstructorId = currentUserId;
                _context.Classrooms.Update(classroom);
                await _context.SaveChangesAsync();

                TempData["success"] = $"✅ Bạn đã nhận dạy lớp '{classroom.NameClass}' thành công!";
                return RedirectToAction(nameof(AvailableClassroomsForInstructor));
            }
            catch (Exception ex)
            {
                TempData["error"] = $"❌ Lỗi: {ex.Message}";
                return RedirectToAction(nameof(AvailableClassroomsForInstructor));
            }
        }
    }
}