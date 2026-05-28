using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public AssignmentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // ==================== INDEX - Danh sách bài tập ====================
        public async Task<IActionResult> Index(int? classroomId)
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách lớp của giáo viên này
            var classrooms = await _context.Set<Classroom>()
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            if (!classrooms.Any())
                return View(new List<Assignment>());

            var query = _context.Set<Assignment>()
                .Include(a => a.Classroom)
                .Include(a => a.StudentAssignments)
                .Where(a => classrooms.Select(c => c.Id).Contains(a.ClassroomId))
                .AsQueryable();

            if (classroomId.HasValue)
            {
                query = query.Where(a => a.ClassroomId == classroomId);
            }

            var assignments = await query.OrderByDescending(a => a.CreatedDate).ToListAsync();

            ViewBag.Classrooms = new SelectList(classrooms, "Id", "NameClass", classroomId);
            return View(assignments);
        }

        // ==================== CREATE - Tạo bài tập mới ====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var classrooms = await _context.Set<Classroom>()
                .Where(c => c.InstructorId == userId && c.IsActive)
                .ToListAsync();

            ViewBag.Classrooms = new SelectList(classrooms, "Id", "NameClass");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment model)
        {
            var userId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                var classrooms = await _context.Set<Classroom>()
                    .Where(c => c.InstructorId == userId && c.IsActive)
                    .ToListAsync();
                ViewBag.Classrooms = new SelectList(classrooms, "Id", "NameClass");
                return View(model);
            }

            // Kiểm tra lớp có thuộc giáo viên này không
            var classroom = await _context.Set<Classroom>()
                .FirstOrDefaultAsync(c => c.Id == model.ClassroomId && c.InstructorId == userId);

            if (classroom == null)
            {
                ModelState.AddModelError("", "Lớp học không tồn tại hoặc không thuộc về bạn");
                return View(model);
            }

            model.InstructorId = userId;
            model.CreatedDate = DateTime.Now;

            _context.Set<Assignment>().Add(model);
            await _context.SaveChangesAsync();

            // Auto-create StudentAssignment cho tất cả học sinh trong lớp
            var classroomStudents = await _context.Set<ClassroomStudent>()
                .Where(cs => cs.ClassroomId == model.ClassroomId)
                .ToListAsync();

            foreach (var student in classroomStudents)
            {
                var studentAssignment = new StudentAssignment
                {
                    AssignmentId = model.Id,
                    StudentId = student.StudentId,
                    IsSubmitted = false
                };
                _context.Set<StudentAssignment>().Add(studentAssignment);
            }

            await _context.SaveChangesAsync();

            // 🔔 Notification: Assignment mới
            var studentIds = classroomStudents.Select(s => s.StudentId).ToList();
            await _notificationService.CreateManyAsync(
                studentIds,
                "Bài tập mới",
                $"Bài tập '{model.Title}' vừa được tạo. Hạn nộp: {model.DueDate:dd/MM/yyyy HH:mm}.",
                "AssignmentNew",
                "Assignment",
                model.Id);

            TempData["success"] = "Tạo bài tập thành công";
            return RedirectToAction(nameof(Index));
        }

        // ==================== EDIT - Sửa bài tập ====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _context.Set<Assignment>()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (assignment.InstructorId != userId)
                return Forbid();

            var classrooms = await _context.Set<Classroom>()
                .Where(c => c.InstructorId == userId && c.IsActive)
                .ToListAsync();

            ViewBag.Classrooms = new SelectList(classrooms, "Id", "NameClass", assignment.ClassroomId);
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Assignment model)
        {
            if (id != model.Id)
                return BadRequest();

            var assignment = await _context.Set<Assignment>()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (assignment.InstructorId != userId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                var classrooms = await _context.Set<Classroom>()
                    .Where(c => c.InstructorId == userId && c.IsActive)
                    .ToListAsync();
                ViewBag.Classrooms = new SelectList(classrooms, "Id", "NameClass");
                return View(model);
            }

            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.Content = model.Content;
            assignment.DueDate = model.DueDate;

            _context.Set<Assignment>().Update(assignment);
            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật bài tập thành công";
            return RedirectToAction(nameof(Index));
        }

        // ==================== DELETE - Xóa bài tập ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.Set<Assignment>()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (assignment.InstructorId != userId)
                return Forbid();

            _context.Set<Assignment>().Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa bài tập thành công";
            return RedirectToAction(nameof(Index));
        }

        // ==================== SUBMISSIONS - Xem bài nộp của học sinh ====================
        public async Task<IActionResult> Submissions(int assignmentId)
        {
            var assignment = await _context.Set<Assignment>()
                .Include(a => a.StudentAssignments)
                    .ThenInclude(sa => sa.Student)
                .Include(a => a.StudentAssignments)
                    .ThenInclude(sa => sa.Submissions)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (assignment.InstructorId != userId)
                return Forbid();

            return View(assignment);
        }

        // ==================== GRADE - Chấm điểm ====================
        [HttpGet]
        public async Task<IActionResult> Grade(int studentAssignmentId)
        {
            var studentAssignment = await _context.Set<StudentAssignment>()
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Instructor)
                .Include(sa => sa.Student)
                .Include(sa => sa.Submissions)
                .FirstOrDefaultAsync(sa => sa.Id == studentAssignmentId);

            if (studentAssignment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (studentAssignment.Assignment?.InstructorId != userId)
                return Forbid();

            return View(studentAssignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int studentAssignmentId, string score, string feedback)
        {
            var studentAssignment = await _context.Set<StudentAssignment>()
                .Include(sa => sa.Assignment)
                .FirstOrDefaultAsync(sa => sa.Id == studentAssignmentId);

            if (studentAssignment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (studentAssignment.Assignment?.InstructorId != userId)
                return Forbid();

            studentAssignment.Score = score;
            studentAssignment.Feedback = feedback;
            studentAssignment.GradedDate = DateTime.Now;

            _context.Set<StudentAssignment>().Update(studentAssignment);
            await _context.SaveChangesAsync();

            // 🔔 Notification: Assignment đã được chấm
            await _notificationService.CreateAsync(new Notification
            {
                RecipientUserId = studentAssignment.StudentId,
                Title = "Bài tập đã được chấm",
                Message = $"Bài tập '{studentAssignment.Assignment?.Title}' đã được chấm điểm. Điểm: {score}.",
                Type = "AssignmentGraded",
                EntityType = "Assignment",
                EntityId = studentAssignment.AssignmentId,
                CreatedAt = DateTime.Now
            });

            TempData["success"] = "Chấm điểm thành công";
            return RedirectToAction(nameof(Submissions), new { assignmentId = studentAssignment.AssignmentId });
        }
    }
}