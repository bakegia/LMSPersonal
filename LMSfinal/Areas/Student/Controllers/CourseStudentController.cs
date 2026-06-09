using LMSfinal.Data;
using LMSfinal.Services;
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
        private readonly IClassroomAccessService _classroomAccessService;

        public CourseStudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IClassroomAccessService classroomAccessService)
        {
            _userManager = userManager;
            _context = context;
            _classroomAccessService = classroomAccessService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var completedLessonIds = await _context.UserProgresses
                .Where(up => up.UserId == userId && up.IsCompleted)
                .Select(up => up.LessonId)
                .ToListAsync();

            var allCourses = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            ViewBag.CompletedLessonIds = completedLessonIds;

            return View(allCourses);
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _classroomAccessService.EnsureStudentAccessAsync(userId, id))
            {
                return RedirectToAction("Locked", "Payment", new { area = "Student", classroomId = id });
            }

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

            int completedCount = await _context.UserProgresses
                .CountAsync(p => p.UserId == userId && p.IsCompleted && lessonIds.Contains(p.LessonId));

            ViewBag.ProgressPercent = totalLessons > 0 ? (int)((double)completedCount / totalLessons * 100) : 0;
            ViewBag.CompletedCount = completedCount;
            ViewBag.TotalLessons = totalLessons;
            return View(classroom);
        }
        public async Task<IActionResult> ViewLesson(int id)
        {
            var userId = _userManager.GetUserId(User);

            var lesson = await _context.Lessons
                .Include(l => l.Section)
                    .ThenInclude(s => s.Classroom)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            var classroomId = lesson.Section?.ClassroomId ?? 0;
            if (classroomId == 0 || !await _classroomAccessService.EnsureStudentAccessAsync(userId, classroomId))
            {
                return RedirectToAction("Locked", "Payment", new { area = "Student", classroomId });
            }

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);

            ViewBag.IsCompleted = progress?.IsCompleted == true;
            ViewBag.VideoCompleted = progress?.IsVideoCompleted == true;
            ViewBag.WatchedPercent = progress?.WatchedPercent ?? 0;
            ViewBag.QuizPassed = progress?.IsQuizPassed == true;

            ViewBag.RequireVideo = lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds > 0;
            ViewBag.RequireQuiz = lesson.RequireQuiz;
            ViewBag.RequireQuizPass = lesson.RequireQuizPass;

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

            ViewBag.CanMarkComplete = await CanMarkCompleteAsync(lesson, progress);

            return View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLessonProgress([FromBody] UpdateLessonProgressRequest request)
        {
            if (request == null || request.LessonId == 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập!" });
            }

            var lesson = await _context.Lessons.FindAsync(request.LessonId);
            if (lesson == null)
            {
                return Json(new { success = false, message = "Bài học không tồn tại!" });
            }

            var progress = await GetOrCreateProgressAsync(userId, request.LessonId);

            if (request.VideoDurationSeconds > 0 && (!lesson.VideoDurationSeconds.HasValue || lesson.VideoDurationSeconds == 0))
            {
                lesson.VideoDurationSeconds = request.VideoDurationSeconds;
            }

            progress.WatchedSeconds = Math.Max(progress.WatchedSeconds, request.WatchedSeconds);
            progress.LastWatchedAt = DateTime.Now;

            var duration = lesson.VideoDurationSeconds ?? 0;
            progress.WatchedPercent = duration > 0
                ? Math.Min(100, (int)Math.Round((double)progress.WatchedSeconds / duration * 100))
                : 0;

            progress.IsVideoCompleted = duration > 0 && progress.WatchedPercent >= lesson.RequiredWatchPercent;

            await _context.SaveChangesAsync();

            var canMarkComplete = await CanMarkCompleteAsync(lesson, progress);

            return Json(new
            {
                success = true,
                watchedPercent = progress.WatchedPercent,
                isVideoCompleted = progress.IsVideoCompleted,
                canMarkComplete
            });
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

                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    return Json(new { success = false, message = "Lỗi: Bài học không tồn tại!" });
                }

                var progress = await GetOrCreateProgressAsync(userId, lessonId);

                if (progress.IsCompleted)
                {
                    return Json(new { success = false, message = "Bài học này đã được ghi nhận rồi." });
                }

                var canMarkComplete = await CanMarkCompleteAsync(lesson, progress);
                if (!canMarkComplete)
                {
                    return Json(new { success = false, message = "Bạn cần xem đủ video và hoàn thành Quiz trước khi đánh dấu." });
                }

                progress.IsCompleted = true;
                progress.CompletedDate ??= DateTime.Now;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LỖI SQL/CODE: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public class UpdateLessonProgressRequest
        {
            public int LessonId { get; set; }
            public int WatchedSeconds { get; set; }
            public int VideoDurationSeconds { get; set; }
        }

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

                var quiz = await _context.Set<Quiz>()
                    .Include(q => q.Questions)
                        .ThenInclude(qn => qn.Answers)
                    .FirstOrDefaultAsync(q => q.Id == model.QuizId);

                if (quiz == null)
                    return Json(new { success = false, message = "Quiz không tồn tại" });

                decimal totalPoints = quiz.Questions.Sum(q => q.Points);

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

                quizAttempt.Score = totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;
                quizAttempt.Passed = quizAttempt.Score >= quiz.PassingScore;

                _context.Set<StudentQuizAttempt>().Add(quizAttempt);

                var progress = await GetOrCreateProgressAsync(userId, quiz.LessonId);
                if (quizAttempt.Passed)
                {
                    progress.IsQuizPassed = true;
                    progress.QuizPassedAt ??= DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var lesson = await _context.Lessons.FindAsync(quiz.LessonId);
                var canMarkComplete = lesson != null && await CanMarkCompleteAsync(lesson, progress);

                return Json(new
                {
                    success = true,
                    passed = quizAttempt.Passed,
                    score = Math.Round(quizAttempt.Score, 2),
                    earnedPoints = earnedPoints,
                    totalPoints = totalPoints,
                    canMarkComplete,
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

        private async Task<UserProgress> GetOrCreateProgressAsync(string userId, int lessonId)
        {
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress != null)
            {
                return progress;
            }

            progress = new UserProgress
            {
                UserId = userId,
                LessonId = lessonId
            };

            _context.UserProgresses.Add(progress);
            return progress;
        }

        private async Task<bool> CanMarkCompleteAsync(Lesson lesson, UserProgress? progress)
        {
            bool videoRequired = lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds > 0;
            bool videoOk = !videoRequired || (progress?.IsVideoCompleted ?? false);

            if (!lesson.RequireQuiz)
            {
                return videoOk;
            }

            bool quizExists = await _context.Set<Quiz>()
                .AnyAsync(q => q.LessonId == lesson.Id && q.IsActive);

            if (!quizExists)
            {
                return false;
            }

            bool quizOk = !lesson.RequireQuizPass || (progress?.IsQuizPassed ?? false);
            return videoOk && quizOk;
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