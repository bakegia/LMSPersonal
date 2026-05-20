using LMSfinal.Data;
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
            .ToListAsync();
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Khởi tạo VM mới và load các danh sách Dropdown
            var vm = await LoadVM();

            // Thiết lập ngày mặc định nếu muốn
            vm.StartDate = DateTime.Now;
            vm.EndDate = DateTime.Now.AddMonths(2);

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassroomVM vm)
        {
            // 1. Kiểm tra ngày học hợp lệ
            if (vm.StartDate >= vm.EndDate)
            {
                ModelState.AddModelError(string.Empty, "Ngày kết thúc phải lớn hơn ngày bắt đầu học.");
            }

            if (!ModelState.IsValid)
            {
                vm = await LoadVM(vm);
                return View(vm);
            }

            // Sử dụng Transaction để đảm bảo tính toàn vẹn dữ liệu tuyệt đối
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
                    InstructorId = vm.InstructorId
                };

                _context.Classrooms.Add(classroom);
                await _context.SaveChangesAsync(); // Lưu để sinh ra classroom.Id

                // 👉 THÊM HỌC SINH VÀO LỚP
                if (vm.StudentIds != null && vm.StudentIds.Any())
                {
                    var classroomStudents = vm.StudentIds.Select(studentId => new ClassroomStudent
                    {
                        ClassroomId = classroom.Id,
                        StudentId = studentId
                    });
                    await _context.ClassroomStudents.AddRangeAsync(classroomStudents);
                }

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
                await transaction.CommitAsync(); // Xác nhận lưu toàn bộ thay đổi thành công

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Nếu có bất kỳ lỗi nào, hủy bỏ toàn bộ dữ liệu vừa thêm tạm
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình tạo lớp học: " + ex.Message);
                vm = await LoadVM(vm);
                return View(vm);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var classroom = await _context.Classrooms
                .Include(x => x.ClassroomStudents)
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

                // 🔥 QUAN TRỌNG: load student đã chọn
                StudentIds = classroom.ClassroomStudents
                    .Select(x => x.StudentId)
                    .ToList()
            };

            vm = await LoadVM(vm);

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ClassroomVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm = await LoadVM(vm);
                return View(vm);
            }

            var classroom = await _context.Classrooms
                .Include(x => x.ClassroomStudents)
                .FirstOrDefaultAsync(x => x.Id == vm.Id);

            if (classroom == null) return NotFound();

            // UPDATE FIELD
            classroom.ClassCode = vm.ClassCode;
            classroom.NameClass = vm.NameClass;
            classroom.Description = vm.Description;
            classroom.StartDate = vm.StartDate;
            classroom.EndDate = vm.EndDate;
            classroom.IsActive = vm.IsActive;
            classroom.CourseId = vm.CourseId;
            classroom.InstructorId = vm.InstructorId;

            // 🔥 XÓA STUDENT CŨ
            _context.ClassroomStudents.RemoveRange(classroom.ClassroomStudents);

            // 🔥 ADD STUDENT MỚI
            if (vm.StudentIds != null)
            {
                foreach (var studentId in vm.StudentIds)
                {
                    _context.ClassroomStudents.Add(new ClassroomStudent
                    {
                        ClassroomId = classroom.Id,
                        StudentId = studentId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DETAIL
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

            return View(classroom);
        }

        // LOAD DROPDOWN
        private async Task<ClassroomVM> LoadVM(ClassroomVM vm = null)
        {
            vm ??= new ClassroomVM();

            vm.Courses = _context.Courses
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                }).ToList();

            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            vm.Instructors = instructors.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.UserName
            }).ToList();

            var students = await _userManager.GetUsersInRoleAsync("Student");
            vm.Students = students.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.UserName
            }).ToList();

            vm.TimeSlots = await _context.TimeSlots
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name + $" ({x.StartTime} - {x.EndTime})"
            }).ToListAsync();

            return vm;
        }
        public async Task<IActionResult> IsActive(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null) return View("notfound");
            classroom.IsActive = !classroom.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }

}


//// CREATE POST
//[HttpPost]

//public async Task<IActionResult> Create(ClassroomVM vm)
//{
//    if (!ModelState.IsValid)
//    {
//        vm = await LoadVM(vm);
//        return View(vm);
//    }

//    var classroom = new Classroom
//    {
//        ClassCode = vm.ClassCode,
//        NameClass = vm.NameClass,
//        Description = vm.Description,
//        StartDate = vm.StartDate,
//        EndDate = vm.EndDate,
//        IsActive = vm.IsActive,
//        CourseId = vm.CourseId,
//        InstructorId = vm.InstructorId
//    };

//    _context.Classrooms.Add(classroom);
//    await _context.SaveChangesAsync();

//    // ADD STUDENTS
//    if (vm.StudentIds != null)
//    {
//        foreach (var studentId in vm.StudentIds)
//        {
//            _context.Add(new ClassroomStudent
//            {
//                ClassroomId = classroom.Id,
//                StudentId = studentId
//            });
//        }

//        await _context.SaveChangesAsync();
//    }

//    return RedirectToAction(nameof(Index));
//}