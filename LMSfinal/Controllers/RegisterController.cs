using LMSfinal.Data;
using LMSfinal.Models.Enums;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels.Student;
using LMSfinal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Controllers
{
    [Authorize]
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IScheduleConflictService _scheduleConflictService;
        private readonly INotificationService _notificationService;
        private readonly IPricingService _pricingService;

        public RegisterController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IScheduleConflictService scheduleConflictService,
            INotificationService notificationService,
            IPricingService pricingService)
        {
            _context = context;
            _userManager = userManager;
            _scheduleConflictService = scheduleConflictService;
            _notificationService = notificationService;
            _pricingService = pricingService;
        }

        // ==================== STUDENT REGISTRATION ====================
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> AvailableClassroomsForStudent()
        {
            var currentUserId = _userManager.GetUserId(User);
            var viewModel = await BuildPreferencePageViewModel(currentUserId);

            return View(viewModel);
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
                .Include(c => c.Course)
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

            var currentPrice = await _pricingService.GetCurrentPriceAsync();
            if (currentPrice <= 0)
            {
                TempData["error"] = "❌ Chưa thiết lập giá tín chỉ.";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            if (classroom.Course == null || classroom.Course.Credits <= 0)
            {
                TempData["error"] = "❌ Khóa học chưa có tín chỉ hợp lệ.";
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
                    StudentId = currentUserId,
                    EnrolledAt = currentDate,
                    PricePerCreditAtEnroll = currentPrice,
                    TotalPrice = currentPrice * classroom.Course.Credits
                };

                var dueDate = classroom.RegistrationDeadline ?? currentDate.AddDays(2);

                var payment = new ClassroomPayment
                {
                    ClassroomId = classroomId,
                    StudentId = currentUserId,
                    Amount = classroomStudent.TotalPrice,
                    DueDate = dueDate,
                    Status = PaymentStatus.Pending,
                    CreatedAt = currentDate
                };

                _context.ClassroomStudents.Add(classroomStudent);
                _context.ClassroomPayments.Add(payment);
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

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentPreferenceRegister(StudentPreferenceInput input)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildPreferencePageViewModel(currentUserId);
                invalidModel.Input = input;
                return View(nameof(AvailableClassroomsForStudent), invalidModel);
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == input.CourseId);
            var timeSlot = await _context.TimeSlots.FirstOrDefaultAsync(t => t.Id == input.TimeSlotId);
            var instructor = string.IsNullOrWhiteSpace(input.InstructorId)
                ? null
                : await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == input.InstructorId);

            if (course == null)
            {
                ModelState.AddModelError(nameof(StudentPreferenceInput.CourseId), "Khóa học không tồn tại.");
            }

            if (timeSlot == null)
            {
                ModelState.AddModelError(nameof(StudentPreferenceInput.TimeSlotId), "Ca học không tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildPreferencePageViewModel(currentUserId);
                invalidModel.Input = input;
                return View(nameof(AvailableClassroomsForStudent), invalidModel);
            }

            var preference = new StudentPreference
            {
                StudentId = currentUserId,
                CourseId = course!.Id,
                TimeSlotId = timeSlot!.Id,
                InstructorId = instructor?.UserId,
                Reason = input.Reason.Trim()
            };

            _context.StudentPreferences.Add(preference);
            await _context.SaveChangesAsync();

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = admins.Select(a => a.Id).ToList();

            var instructorName = instructor?.FullName ?? "Không yêu cầu";
            var title = "Nguyện vọng lớp học mới";
            var message = $"Sinh viên {User.Identity?.Name} gửi nguyện vọng: Môn {course.Title}, Ca {timeSlot.Name}, GV {instructorName}.";

            await _notificationService.CreateManyAsync(
                adminIds,
                title,
                message,
                "StudentPreference",
                "StudentPreference",
                preference.Id);

            TempData["success"] = "✅ Đã gửi nguyện vọng. Vui lòng chờ admin xét duyệt.";
            return RedirectToAction(nameof(AvailableClassroomsForStudent));
        }

        private async Task<StudentPreferencePageViewModel> BuildPreferencePageViewModel(string currentUserId)
        {
            var currentDate = DateTime.Now;

            var classrooms = await _context.Classrooms
                .Where(c => c.IsOpenForRegistration &&
                            c.InstructorId != null &&
                            c.IsActive)
                .Include(c => c.Course)
                .Include(c => c.Instructor)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.TimeSlot)
                .Include(c => c.ClassroomStudents)
                .OrderBy(c => c.StartDate)
                .ToListAsync();

            var userEnrolledClassroomIds = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == currentUserId)
                .Select(cs => cs.ClassroomId)
                .ToListAsync();

            var userEnrolledCourseIds = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == currentUserId)
                .Select(cs => cs.Classroom.CourseId)
                .ToListAsync();

            ViewBag.UserEnrolledClassroomIds = userEnrolledClassroomIds;
            ViewBag.UserEnrolledCourseIds = userEnrolledCourseIds;

            var courses = await _context.Courses
                .OrderBy(c => c.Title)
                .ToListAsync();

            var timeSlots = await _context.TimeSlots
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            var instructors = await _context.Instructors
                .OrderBy(i => i.FullName)
                .ToListAsync();

            var preferences = await _context.StudentPreferences
                .Where(p => p.StudentId == currentUserId)
                .Include(p => p.Course)
                .Include(p => p.TimeSlot)
                .Include(p => p.Instructor)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return new StudentPreferencePageViewModel
            {
                Classrooms = classrooms,
                Courses = new SelectList(courses, "Id", "Title"),
                TimeSlots = new SelectList(timeSlots, "Id", "Name"),
                Instructors = new SelectList(instructors, "UserId", "FullName"),
                Preferences = preferences
            };
        }

        /// <summary>
        /// Học sinh hủy đăng ký lớp học
        /// </summary>
        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> StudentUnregister(int classroomId)
        {
            var currentUserId = _userManager.GetUserId(User);
            
            var enrollment = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == currentUserId);

            if (enrollment == null)
            {
                TempData["error"] = "❌ Bạn chưa đăng ký lớp học này.";
                return RedirectToAction(nameof(AvailableClassroomsForStudent));
            }

            try
            {
                _context.ClassroomStudents.Remove(enrollment);
                await _context.SaveChangesAsync();

                TempData["success"] = "✅ Đã hủy đăng ký lớp học thành công.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"❌ Lỗi khi hủy đăng ký: {ex.Message}";
            }

            return RedirectToAction(nameof(AvailableClassroomsForStudent));
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