using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels.Student;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    public class CourseStudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseStudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Lấy danh sách bài học sinh viên đã hoàn thành
            var completedLessonIds = await _context.UserProgresses
                .Where(up => up.UserId == userId)
                .Select(up => up.LessonId)
                .ToListAsync();

            // 2. Lấy danh sách lớp học (Cần Include sâu đến tận Lesson để đếm tổng số bài)
            var allCourses = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            // Truyền danh sách ID bài đã xong qua ViewBag
            ViewBag.CompletedLessonIds = completedLessonIds;

            return View(allCourses);
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Lấy trực tiếp đối tượng Classroom từ database
            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null)
            {
                return NotFound();
            }
            var lessonIds = classroom.Sections.SelectMany(s => s.Lessons).Select(l => l.Id).ToList();
            int totalLessons = lessonIds.Count;

            // 2. Đếm số bài học người dùng này đã hoàn thành trong khóa này
            int completedCount = await _context.UserProgresses
                .CountAsync(p => p.UserId == userId && lessonIds.Contains(p.LessonId));

            // 3. Tính phần trăm và truyền qua ViewBag
            ViewBag.ProgressPercent = totalLessons > 0 ? (int)((double)completedCount / totalLessons * 100) : 0;
            ViewBag.CompletedCount = completedCount;
            ViewBag.TotalLessons = totalLessons;
            return View(classroom); // Truyền trực tiếp Model Classroom sang View
        }
        public async Task<IActionResult> ViewLesson(int id)
        {
            var userId = _userManager.GetUserId(User);
            // Lấy bài học cùng với thông tin chương (Section)
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                    .ThenInclude(s => s.Classroom)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            // 1. Kiểm tra bài học đã hoàn thành chưa
            ViewBag.IsCompleted = await _context.UserProgresses
                .AnyAsync(p => p.UserId == userId && p.LessonId == id);

            // 2. Kiểm tra bài này có Quiz hay không
            var quiz = await _context.Set<Quiz>()
                .FirstOrDefaultAsync(q => q.LessonId == id && q.IsActive);

            bool hasQuiz = quiz != null;
            ViewBag.HasQuiz = hasQuiz;
            ViewBag.HasAttempted = false;
            if (hasQuiz)
            {
                ViewBag.HasAttempted = await _context.Set<StudentQuizAttempt>()
                    .AnyAsync(a => a.StudentId == userId && a.QuizId == quiz.Id);
            }

            return View(lesson);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted([FromBody] MarkCompletedRequest request)
        {
            try
            {
                if (request == null || request.LessonId == 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                int lessonId = request.LessonId;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Lỗi: Chưa đăng nhập!" });
                }

                // Kiểm tra bài học tồn tại
                var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == lessonId);
                if (!lessonExists)
                {
                    return Json(new { success = false, message = "Lỗi: Bài học không tồn tại!" });
                }

                var exists = await _context.UserProgresses
                    .AnyAsync(p => p.UserId == userId && p.LessonId == lessonId);

                if (!exists)
                {
                    var progress = new UserProgress
                    {
                        UserId = userId,
                        LessonId = lessonId,
                        CompletedDate = DateTime.Now
                    };
                    _context.UserProgresses.Add(progress);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Bài học này đã được ghi nhận rồi." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LỖI SQL/CODE: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // Thêm Model này vào cuối controller cùng với các model input khác
        public class MarkCompletedRequest
        {
            public int LessonId { get; set; }
        }

        // ==================== GET QUIZ FOR LESSON ====================
        public async Task<IActionResult> GetLessonQuiz(int lessonId)
        {
            var quiz = await _context.Set<Quiz>()
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.Answers)
                .FirstOrDefaultAsync(q => q.LessonId == lessonId && q.IsActive);

            if (quiz == null)
                return NotFound(new { message = "Bài tập này không có Quiz" });

            var quizDto = new
            {
                quiz.Id,
                quiz.Title,
                quiz.Description,
                quiz.PassingScore,
                Questions = quiz.Questions.OrderBy(q => q.Order).Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.Points,
                    Answers = q.Answers.OrderBy(a => a.Order).Select(a => new
                    {
                        a.Id,
                        a.AnswerText,
                        a.AnswerLabel
                        // Không truyền IsCorrect (để ko cheating)
                    }).ToList()
                }).ToList()
            };

            return Json(quizDto);
        }

        // ==================== SUBMIT QUIZ ====================
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Chưa đăng nhập" });

                if (model == null || model.QuizId == 0)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                // 1. Lấy Quiz và tất cả questions + answers
                var quiz = await _context.Set<Quiz>()
                    .Include(q => q.Questions)
                        .ThenInclude(qn => qn.Answers)
                    .FirstOrDefaultAsync(q => q.Id == model.QuizId);

                if (quiz == null)
                    return Json(new { success = false, message = "Quiz không tồn tại" });

                // 2. Tính tổng điểm
                decimal totalPoints = quiz.Questions.Sum(q => q.Points);

                // 3. Chấm điểm tự động
                decimal earnedPoints = 0;
                var quizAttempt = new StudentQuizAttempt
                {
                    StudentId = userId,
                    QuizId = model.QuizId,
                    TotalPoints = totalPoints,
                    AttemptedAt = DateTime.Now,
                    Answers = new List<StudentQuizAnswer>()
                };

                foreach (var studentAnswer in model.Answers)
                {
                    var question = quiz.Questions.FirstOrDefault(q => q.Id == studentAnswer.QuestionId);
                    if (question == null) continue;

                    var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == studentAnswer.SelectedAnswerId);
                    var isCorrect = selectedAnswer?.IsCorrect ?? false;

                    var qAnswerRecord = new StudentQuizAnswer
                    {
                        Attempt = quizAttempt,

                        QuestionId = studentAnswer.QuestionId,

                        SelectedAnswerId = studentAnswer.SelectedAnswerId,

                        IsCorrect = isCorrect,

                        EarnedPoints = isCorrect ? question.Points : 0
                    };

                    quizAttempt.Answers.Add(qAnswerRecord);

                    if (isCorrect)
                        earnedPoints += question.Points;
                }

                // 4. Tính score (%)
                quizAttempt.Score = totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;
                quizAttempt.Passed = quizAttempt.Score >= quiz.PassingScore;

                // 5. Lưu vào database
                _context.Set<StudentQuizAttempt>().Add(quizAttempt);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    passed = quizAttempt.Passed,
                    score = Math.Round(quizAttempt.Score, 2),
                    earnedPoints = earnedPoints,
                    totalPoints = totalPoints,
                    message = quizAttempt.Passed
                        ? $"Chúc mừng! Bạn đạt {quizAttempt.Score:F1}%"
                        : $"Chưa đạt. Bạn được {quizAttempt.Score:F1}% (cần {quiz.PassingScore}%)"
                });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;

                return Json(new
                {
                    success = false,
                    message = inner
                });
            }
        }

        // INPUT MODEL
        public class StudentAnswerInput
        {
            public int QuestionId { get; set; }
            public int SelectedAnswerId { get; set; }
        }
        public class QuizSubmissionModel
        {
            public int QuizId { get; set; }
            public List<StudentAnswerInput> Answers { get; set; } = new List<StudentAnswerInput>();
        }
    }
}
