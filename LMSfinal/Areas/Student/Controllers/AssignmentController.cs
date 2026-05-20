using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AssignmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==================== INDEX - Danh sách bài tập ====================
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var now = DateTime.Now;

            var allAssignments = await _context.Set<StudentAssignment>()
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Classroom)
                        .ThenInclude(c => c.Course)
                .Where(sa => sa.StudentId == userId)
                .ToListAsync();

            var viewModel = new StudentAssignmentIndexViewModel
            {
                AllAssignments = allAssignments.Select(sa => new StudentAssignmentListItemVM
                {
                    Id = sa.AssignmentId,
                    CourseId = sa.Assignment.Classroom.CourseId,
                    CourseTitle = sa.Assignment.Classroom.Course?.Title ?? "",
                    Title = sa.Assignment.Title,
                    Description = sa.Assignment.Description ?? "",
                    DueDate = sa.Assignment.DueDate,
                    IsSubmitted = sa.IsSubmitted,
                    SubmittedAt = sa.FirstSubmittedAt,
                    Score = sa.Score
                }).ToList(),

                UpcomingAssignments = allAssignments
                    .Where(sa => !sa.IsSubmitted && sa.Assignment.DueDate > now)
                    .OrderBy(sa => sa.Assignment.DueDate)
                    .Select(sa => new StudentAssignmentListItemVM
                    {
                        Id = sa.AssignmentId,
                        CourseId = sa.Assignment.Classroom.CourseId,
                        CourseTitle = sa.Assignment.Classroom.Course?.Title ?? "",
                        Title = sa.Assignment.Title,
                        Description = sa.Assignment.Description ?? "",
                        DueDate = sa.Assignment.DueDate,
                        IsSubmitted = sa.IsSubmitted,
                        SubmittedAt = sa.FirstSubmittedAt,
                        Score = sa.Score
                    }).ToList(),

                OverdueAssignments = allAssignments
                    .Where(sa => !sa.IsSubmitted && sa.Assignment.DueDate <= now)
                    .OrderByDescending(sa => sa.Assignment.DueDate)
                    .Select(sa => new StudentAssignmentListItemVM
                    {
                        Id = sa.AssignmentId,
                        CourseId = sa.Assignment.Classroom.CourseId,
                        CourseTitle = sa.Assignment.Classroom.Course?.Title ?? "",
                        Title = sa.Assignment.Title,
                        Description = sa.Assignment.Description ?? "",
                        DueDate = sa.Assignment.DueDate,
                        IsSubmitted = sa.IsSubmitted,
                        SubmittedAt = sa.FirstSubmittedAt,
                        Score = sa.Score
                    }).ToList(),

                SubmittedAssignments = allAssignments
                    .Where(sa => sa.IsSubmitted)
                    .OrderByDescending(sa => sa.LastSubmittedAt)
                    .Select(sa => new StudentAssignmentListItemVM
                    {
                        Id = sa.AssignmentId,
                        CourseId = sa.Assignment.Classroom.CourseId,
                        CourseTitle = sa.Assignment.Classroom.Course?.Title ?? "",
                        Title = sa.Assignment.Title,
                        Description = sa.Assignment.Description ?? "",
                        DueDate = sa.Assignment.DueDate,
                        IsSubmitted = sa.IsSubmitted,
                        SubmittedAt = sa.LastSubmittedAt,
                        Score = sa.Score
                    }).ToList()
            };

            return View(viewModel);
        }

        // ==================== DETAILS - Xem chi tiết bài tập ====================
        // Thay phần Details method
        public async Task<IActionResult> Details(int studentAssignmentId)
        {
            var userId = _userManager.GetUserId(User);

            var studentAssignment = await _context.Set<StudentAssignment>()
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Classroom)
                        .ThenInclude(c => c.Course)
                .Include(sa => sa.Submissions)
                .FirstOrDefaultAsync(sa => sa.Id == studentAssignmentId && sa.StudentId == userId);

            if (studentAssignment == null)
                return NotFound();

            var now = DateTime.Now;
            var isOverdue = now > studentAssignment.Assignment.DueDate;

            var viewModel = new StudentAssignmentViewDetailViewModel
            {
                AssignmentDetail = new StudentAssignmentDetailsViewModel
                {
                    Id = studentAssignment.AssignmentId,
                    CourseId = studentAssignment.Assignment.Classroom.CourseId,
                    CourseTitle = studentAssignment.Assignment.Classroom.Course?.Title ?? "",
                    Title = studentAssignment.Assignment.Title,
                    Description = studentAssignment.Assignment.Description ?? "",
                    Content = studentAssignment.Assignment.Content,
                    DueDate = studentAssignment.Assignment.DueDate,
                    IsSubmitted = studentAssignment.IsSubmitted,
                    SubmittedAt = studentAssignment.LastSubmittedAt,
                    SubmissionText = studentAssignment.Submissions.LastOrDefault()?.SubmissionText,
                    Score = studentAssignment.Score
                },
                Submissions = studentAssignment.Submissions
                    .OrderByDescending(s => s.SubmittedAt)
                    .Select(s => new StudentAssignmentSubmissionItemVM
                    {
                        SubmissionId = s.Id,
                        SubmittedAt = s.SubmittedAt,
                        SubmissionText = s.SubmissionText,
                        AttachmentUrl = s.AttachmentUrl,
                        IsLate = s.IsLate
                    }).ToList(),
                IsOverdue = isOverdue,
                IsLateSubmission = isOverdue && !studentAssignment.IsSubmitted
            };

            // 🔥 THÊM DÒNG NÀY
            ViewBag.StudentAssignmentId = studentAssignmentId;

            return View(viewModel);
        }

        // ==================== SUBMIT - Nộp bài ====================
        [HttpGet]
        public async Task<IActionResult> Submit(int studentAssignmentId)
        {
            var userId = _userManager.GetUserId(User);

            var studentAssignment = await _context.Set<StudentAssignment>()
                .Include(sa => sa.Assignment)
                    .ThenInclude(a => a.Classroom)
                        .ThenInclude(c => c.Course)
                .Include(sa => sa.Submissions)
                .FirstOrDefaultAsync(sa => sa.Id == studentAssignmentId && sa.StudentId == userId);

            if (studentAssignment == null)
                return NotFound();

            var model = new StudentAssignmentSubmitViewModel
            {
                AssignmentId = studentAssignment.AssignmentId,
                StudentAssignmentId = studentAssignment.Id,
                CourseTitle = studentAssignment.Assignment.Classroom.Course?.Title ?? "",
                AssignmentTitle = studentAssignment.Assignment.Title,
                Description = studentAssignment.Assignment.Description ?? "",
                Content = studentAssignment.Assignment.Content,
                DueDate = studentAssignment.Assignment.DueDate,
                HasPreviousSubmission = studentAssignment.IsSubmitted,
                PreviousSubmissionText = studentAssignment.Submissions.LastOrDefault()?.SubmissionText
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int studentAssignmentId, StudentAssignmentSubmitViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var studentAssignment = await _context.Set<StudentAssignment>()
                .Include(sa => sa.Assignment)
                .FirstOrDefaultAsync(sa => sa.Id == studentAssignmentId && sa.StudentId == userId);

            if (studentAssignment == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.SubmissionText) && model.AttachmentFile == null)
            {
                ModelState.AddModelError("", "Vui lòng nhập nội dung hoặc đính kèm file");
                return View(model);
            }

            string? attachmentUrl = null;

            // Xử lý upload file nếu có
            if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "assignments");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{model.AttachmentFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AttachmentFile.CopyToAsync(fileStream);
                }

                attachmentUrl = $"/uploads/assignments/{fileName}";
            }

            var now = DateTime.Now;
            var isLate = now > studentAssignment.Assignment.DueDate;

            var submission = new AssignmentSubmission
            {
                StudentAssignmentId = studentAssignment.Id,
                SubmissionText = model.SubmissionText,
                AttachmentUrl = attachmentUrl,
                SubmittedAt = now,
                IsLate = isLate
            };

            _context.Set<AssignmentSubmission>().Add(submission);

            // Cập nhật StudentAssignment
            studentAssignment.IsSubmitted = true;
            if (studentAssignment.FirstSubmittedAt == null)
                studentAssignment.FirstSubmittedAt = now;
            studentAssignment.LastSubmittedAt = now;

            _context.Set<StudentAssignment>().Update(studentAssignment);
            await _context.SaveChangesAsync();

            TempData["success"] = isLate ? "Nộp bài thành công (trễ hạn)" : "Nộp bài thành công";
            return RedirectToAction(nameof(Details), new { studentAssignmentId = studentAssignment.Id });
        }
    }
}