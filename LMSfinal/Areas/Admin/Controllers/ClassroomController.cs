using LMSfinal.Data;
using LMSfinal.Models.Enums;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassroomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ClassroomController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var data = await _context.Classrooms
                .Include(x => x.Course)
                .Include(x => x.Instructor)
                .Include(x => x.ClassroomStudents)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

            // ✅ FIX: Set default MaxCapacity = 30 nếu = 0
            foreach (var classroom in data)
            {
                if (classroom.MaxCapacity <= 0)
                {
                    classroom.MaxCapacity = 30;
                }
            }

            return View(data);
        }

        // ==================== CREATE GET ====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await LoadVM();

            // Thiết lập giá trị mặc định
            vm.StartDate = DateTime.Now;
            vm.EndDate = DateTime.Now.AddMonths(2);
            vm.RegistrationDeadline = DateTime.Now.AddDays(2); // Mặc định: 7 ngày
            vm.MaxCapacity = 30; // Mặc định: 30 học sinh
            vm.IsOpenForRegistration = true; // Mặc định: mở đăng ký

            return View(vm);
        }

        // ==================== CREATE POST ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassroomVM vm)
        {
            // ===== VALIDATION =====
            if (vm.StartDate >= vm.EndDate)
            {
                ModelState.AddModelError(string.Empty, "❌ Ngày kết thúc phải lớn hơn ngày bắt đầu học.");
            }

            if (vm.RegistrationDeadline.HasValue && vm.RegistrationDeadline.Value < DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "❌ Hạn đăng ký phải lớn hơn ngày hôm nay.");
            }

            if (vm.MaxCapacity < 1 || vm.MaxCapacity > 200)
            {
                ModelState.AddModelError(string.Empty, "❌ Sức chứa phải từ 1 đến 200 học sinh.");
            }

            if (!ModelState.IsValid)
            {
                vm = await LoadVM(vm);
                return View(vm);
            }

            // ===== TRANSACTION =====
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var classroom = new Classroom
                {
                    ClassCode = vm.ClassCode,
                    NameClass = vm.NameClass,
                    Description = vm.Description,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    IsActive = vm.IsActive,
                    CourseId = vm.CourseId,

                    // ✅ THÊM CÁC TRƯỜNG ĐĂNG KÝ
                    IsOpenForRegistration = vm.IsOpenForRegistration,
                    MaxCapacity = vm.MaxCapacity,
                    RegistrationDeadline = vm.RegistrationDeadline,

                    // ⚠️ GIÁO VIÊN & HỌC SINH = NULL
                    InstructorId = vm.InstructorId
                };

                _context.Classrooms.Add(classroom);
                await _context.SaveChangesAsync(); // Lưu để sinh ra classroom.Id

                // 👉 THÊM THỜI KHÓA BIỂU
                if (vm.SelectedDays != null && vm.SelectedDays.Any())
                {
                    var schedules = vm.SelectedDays.Select(day => new ClassSchedule
                    {
                        ClassroomId = classroom.Id,
                        TimeSlotId = vm.TimeSlotId,
                        DayOfWeek = day
                    });
                    await _context.ClassSchedules.AddRangeAsync(schedules);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["success"] = $"✅ Tạo lớp '{classroom.NameClass}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"❌ Lỗi: {ex.Message}");
                vm = await LoadVM(vm);
                return View(vm);
            }
        }

        // ==================== EDIT GET ====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var classroom = await _context.Classrooms
                .Include(x => x.ClassroomStudents)
                .Include(x => x.ClassSchedules)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (classroom == null) return NotFound();

            var vm = new ClassroomVM
            {
                Id = classroom.Id,
                ClassCode = classroom.ClassCode,
                NameClass = classroom.NameClass,
                Description = classroom.Description,
                StartDate = classroom.StartDate,
                EndDate = classroom.EndDate,
                IsActive = classroom.IsActive,
                CourseId = classroom.CourseId,
                InstructorId = classroom.InstructorId,

                // ✅ LOAD ĐĂNG KÝ FIELDS
                IsOpenForRegistration = classroom.IsOpenForRegistration,
                MaxCapacity = classroom.MaxCapacity,
                RegistrationDeadline = classroom.RegistrationDeadline,

                // Load học sinh đã chọn
                StudentIds = classroom.ClassroomStudents
                    .Select(x => x.StudentId)
                    .ToList(),

                // Load những ngày đã chọn
                SelectedDays = classroom.ClassSchedules
                    .Select(x => x.DayOfWeek)
                    .Distinct()
                    .ToList(),

                // Load TimeSlot đã chọn (lấy cái đầu tiên nếu có)
                TimeSlotId = classroom.ClassSchedules.FirstOrDefault()?.TimeSlotId ?? 0
            };

            vm = await LoadVM(vm);
            return View(vm);
        }

        // ==================== EDIT POST ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassroomVM vm)
        {
            // ===== VALIDATION =====
            if (vm.StartDate >= vm.EndDate)
            {
                ModelState.AddModelError(string.Empty, "❌ Ngày kết thúc phải lớn hơn ngày bắt đầu học.");
            }

            if (vm.MaxCapacity < 1 || vm.MaxCapacity > 200)
            {
                ModelState.AddModelError(string.Empty, "❌ Sức chứa phải từ 1 đến 200 học sinh.");
            }

            if (!ModelState.IsValid)
            {
                vm = await LoadVM(vm);
                return View(vm);
            }

            var classroom = await _context.Classrooms
                .Include(x => x.ClassroomStudents)
                .Include(x => x.ClassSchedules)
                .FirstOrDefaultAsync(x => x.Id == vm.Id);

            if (classroom == null) return NotFound();

            // ===== UPDATE FIELDS =====
            classroom.ClassCode = vm.ClassCode;
            classroom.NameClass = vm.NameClass;
            classroom.Description = vm.Description;
            classroom.StartDate = vm.StartDate;
            classroom.EndDate = vm.EndDate;
            classroom.IsActive = vm.IsActive;
            classroom.CourseId = vm.CourseId;

            // ✅ UPDATE ĐĂNG KÝ FIELDS
            classroom.IsOpenForRegistration = vm.IsOpenForRegistration;
            classroom.MaxCapacity = vm.MaxCapacity;
            classroom.RegistrationDeadline = vm.RegistrationDeadline;

            // ⚠️ KHÔNG CẬP NHẬT InstructorId (giáo viên đăng ký)

            // ===== UPDATE THỜI KHÓA BIỂU =====
            // Xóa lịch cũ
            _context.ClassSchedules.RemoveRange(classroom.ClassSchedules);

            // Thêm lịch mới
            if (vm.SelectedDays != null && vm.SelectedDays.Any())
            {
                var newSchedules = vm.SelectedDays.Select(day => new ClassSchedule
                {
                    ClassroomId = classroom.Id,
                    TimeSlotId = vm.TimeSlotId,
                    DayOfWeek = day
                });
                await _context.ClassSchedules.AddRangeAsync(newSchedules);
            }

            await _context.SaveChangesAsync();

            TempData["success"] = $"✅ Cập nhật lớp '{classroom.NameClass}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==================== DETAILS ====================
        public async Task<IActionResult> Details(int id)
        {
            var classroom = await _context.Classrooms
                .Include(x => x.Course)
                .Include(x => x.Instructor)
                .Include(x => x.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .Include(x => x.ClassSchedules)
                    .ThenInclude(cs => cs.TimeSlot)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (classroom == null) return NotFound();
            var mssv = await _context.UserProfiles
                .Where(up => classroom.ClassroomStudents.Select(cs => cs.StudentId).Contains(up.UserId))
                .ToDictionaryAsync(up => up.UserId, up => up.Mssv);
            ViewBag.MssvLookup = mssv; // Truyền dictionary UserId -> MSSV cho View để hiển thị
            return View(classroom);
        }

        // ==================== DELETE ====================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var classroom = await _context.Classrooms
                .Include(x => x.ClassroomStudents)
                .Include(x => x.ClassSchedules)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (classroom == null)
            {
                TempData["error"] = "❌ Lớp học không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Xóa liên kết
                _context.ClassroomStudents.RemoveRange(classroom.ClassroomStudents);
                _context.ClassSchedules.RemoveRange(classroom.ClassSchedules);

                // Xóa lớp
                _context.Classrooms.Remove(classroom);
                await _context.SaveChangesAsync();

                TempData["success"] = $"✅ Xóa lớp '{classroom.NameClass}' thành công!";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"❌ Lỗi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================== TOGGLE ACTIVE ====================
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return Json(new { success = false, message = "Lớp không tồn tại" });
            }

            classroom.IsActive = !classroom.IsActive;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = classroom.IsActive,
                message = classroom.IsActive ? "✅ Lớp đã được kích hoạt" : "⚠️ Lớp đã bị vô hiệu hóa"
            });
        }

        // ==================== TOGGLE REGISTRATION ====================
        [HttpPost]
        public async Task<IActionResult> ToggleRegistration(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return Json(new { success = false, message = "Lớp không tồn tại" });
            }

            classroom.IsOpenForRegistration = !classroom.IsOpenForRegistration;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isOpen = classroom.IsOpenForRegistration,
                message = classroom.IsOpenForRegistration ? "✅ Mở đăng ký thành công" : "⚠️ Đóng đăng ký thành công"
            });
        }
        private async Task<ClassroomVM> LoadVM(ClassroomVM vm = null)
        {
            vm ??= new ClassroomVM();

            // Courses
            vm.Courses = _context.Courses
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();

            // Instructors
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            vm.Instructors = instructors.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.UserName
            }).ToList();

            // Students
            var students = await _userManager.GetUsersInRoleAsync("Student");
            vm.Students = students.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.UserName
            }).ToList();

            // TimeSlots - ✅ FIX: Format TimeSpan một cách an toàn
            vm.TimeSlots = await _context.TimeSlots
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    // ✅ Dùng ToString("hh\\:mm") thay vì :HH\\:mm
                    Text = $"{x.Name} ({x.StartTime.ToString(@"hh\:mm")} - {x.EndTime.ToString(@"hh\:mm")})"
                }).ToListAsync();

            return vm;
        }
        // ==================== GET AVAILABLE STUDENTS (API for Select2) ====================
        [HttpGet]
        public async Task<IActionResult> GetAvailableStudents(int classroomId, string q)
        {
            // Lấy danh sách ID sinh viên đã có trong lớp này
            var enrolledStudentIds = await _context.ClassroomStudents
                .Where(cs => cs.ClassroomId == classroomId)
                .Select(cs => cs.StudentId)
                .ToListAsync();

            // Truy vấn danh sách Profile (chứa MSSV và Fullname) chưa có trong lớp
            var query = _context.UserProfiles
                .Where(u => !enrolledStudentIds.Contains(u.UserId));

            // Lọc theo từ khóa (MSSV hoặc Tên)
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(u => u.Mssv.ToString().Contains(q) || u.Fullname.Contains(q));
            }

            var data = await query
                .Take(20) // Giới hạn số lượng trả về để đạt hiệu suất
                .Select(u => new
                {
                    id = u.UserId,
                    text = $"[{u.Mssv}] - {u.Fullname}"
                })
                .ToListAsync();

            return Json(new { results = data });
        }

        // ==================== ADD STUDENTS TO CLASS (POST) ====================
        [HttpPost]
        public async Task<IActionResult> AddStudentsToClass(int classroomId, List<string> studentIds)
        {
            if (studentIds == null || !studentIds.Any())
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một sinh viên." });
            }

            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.ClassroomStudents)
                .FirstOrDefaultAsync(c => c.Id == classroomId);

            if (classroom == null)
            {
                return Json(new { success = false, message = "Lớp học không tồn tại." });
            }

            int currentCount = classroom.ClassroomStudents.Count;
            if (currentCount + studentIds.Count > classroom.MaxCapacity)
            {
                return Json(new
                {
                    success = false,
                    message = $"Không thể thêm. Sĩ số hiện tại {currentCount}/{classroom.MaxCapacity}. Bạn đang cố thêm {studentIds.Count} sinh viên."
                });
            }

            var now = DateTime.Now;

            var pricePerCredit = await _context.PricePerCreditHistories
                .Where(p => p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo >= now))
                .OrderByDescending(p => p.EffectiveFrom)
                .Select(p => p.Price)
                .FirstOrDefaultAsync();

            if (pricePerCredit <= 0)
            {
                return Json(new { success = false, message = "Chưa thiết lập giá tín chỉ hợp lệ." });
            }

            if (classroom.Course == null || classroom.Course.Credits <= 0)
            {
                return Json(new { success = false, message = "Môn học chưa có tín chỉ hợp lệ." });
            }

            try
            {
                var newEnrolments = new List<ClassroomStudent>();
                var newPayments = new List<ClassroomPayment>();

                foreach (var studentId in studentIds)
                {
                    if (classroom.ClassroomStudents.Any(cs => cs.StudentId == studentId))
                    {
                        continue;
                    }

                    var totalPrice = pricePerCredit * classroom.Course.Credits;

                    var enrollment = new ClassroomStudent
                    {
                        ClassroomId = classroomId,
                        StudentId = studentId,
                        EnrolledAt = now,
                        PricePerCreditAtEnroll = pricePerCredit,
                        TotalPrice = totalPrice
                    };

                    newEnrolments.Add(enrollment);

                    var paymentExists = await _context.ClassroomPayments
                        .AnyAsync(p => p.ClassroomId == classroomId && p.StudentId == studentId);

                    if (!paymentExists)
                    {
                        var dueDate = classroom.RegistrationDeadline ?? now.AddDays(2);

                        newPayments.Add(new ClassroomPayment
                        {
                            ClassroomId = classroomId,
                            StudentId = studentId,
                            Amount = totalPrice,
                            DueDate = dueDate,
                            Status = PaymentStatus.Pending,
                            CreatedAt = now
                        });
                    }
                }

                if (newEnrolments.Any())
                {
                    _context.ClassroomStudents.AddRange(newEnrolments);
                }

                if (newPayments.Any())
                {
                    _context.ClassroomPayments.AddRange(newPayments);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã thêm thành công {newEnrolments.Count} sinh viên vào lớp." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}