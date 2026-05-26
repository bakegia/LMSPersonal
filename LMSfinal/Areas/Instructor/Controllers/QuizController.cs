using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== INDEX - Danh sách Quiz ====================
        public async Task<IActionResult> Index(int? classroomId)
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách lớp của giáo viên
            var classrooms = await _context.Set<Classroom>()
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            if (!classrooms.Any())
                return View(new List<Quiz>());

            var classroomIds = classrooms.Select(c => c.Id).ToList();

            // Lấy Quiz từ các Lesson thuộc Classroom của giáo viên
            var query = _context.Set<Quiz>()
                .Include(q => q.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                .Include(q => q.Questions)
                .Include(q => q.StudentAttempts)
                .Where(q => classroomIds.Contains(q.Lesson.Section.Classroom.Id))
                .AsQueryable();

            if (classroomId.HasValue)
            {
                query = query.Where(q => q.Lesson.Section.Classroom.Id == classroomId);
            }

            var quizzes = await query.OrderByDescending(q => q.CreatedDate).ToListAsync();

            ViewBag.Classrooms = new SelectList(classrooms, "Id", "NameClass", classroomId);
            return View(quizzes);
        }

        // ==================== CREATE - Tạo Quiz mới ====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            // Lấy danh sách Lesson từ các Classroom của giáo viên
            var lessons = await _context.Set<Lesson>()
                .Include(l => l.Section)
                    .ThenInclude(s => s.Classroom)
                .Where(l => l.Section.Classroom.InstructorId == userId)
                .ToListAsync();

            ViewBag.Lessons = new SelectList(lessons, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz model)
        {
            var userId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                var lessons = await _context.Set<Lesson>()
                    .Include(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                    .Where(l => l.Section.Classroom.InstructorId == userId)
                    .ToListAsync();
                ViewBag.Lessons = new SelectList(lessons, "Id", "Title");
                return View(model);
            }

            // Kiểm tra Lesson có thuộc giáo viên không
            var lesson = await _context.Set<Lesson>()
                .Include(l => l.Section)
                    .ThenInclude(s => s.Classroom)
                .FirstOrDefaultAsync(l => l.Id == model.LessonId && l.Section.Classroom.InstructorId == userId);

            if (lesson == null)
            {
                ModelState.AddModelError("", "Bài học không tồn tại hoặc không thuộc về bạn");
                var lessons = await _context.Set<Lesson>()
                    .Include(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                    .Where(l => l.Section.Classroom.InstructorId == userId)
                    .ToListAsync();
                ViewBag.Lessons = new SelectList(lessons, "Id", "Title");
                return View(model);
            }

            model.CreatedDate = DateTime.Now;
            model.IsActive = true;

            _context.Set<Quiz>().Add(model);
            await _context.SaveChangesAsync();

            TempData["success"] = "Tạo Quiz thành công";
            return RedirectToAction(nameof(ManageQuestions), new { quizId = model.Id });
        }

        // ==================== EDIT - Sửa Quiz ====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _context.Set<Quiz>()
                .Include(q => q.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (quiz.Lesson.Section.Classroom.InstructorId != userId)
                return Forbid();

            var lessons = await _context.Set<Lesson>()
                .Include(l => l.Section)
                    .ThenInclude(s => s.Classroom)
                .Where(l => l.Section.Classroom.InstructorId == userId)
                .ToListAsync();

            ViewBag.Lessons = new SelectList(lessons, "Id", "Title", quiz.LessonId);
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quiz model)
        {
            if (id != model.Id)
                return BadRequest();

            var quiz = await _context.Set<Quiz>()
                .Include(q => q.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (quiz.Lesson.Section.Classroom.InstructorId != userId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                var lessons = await _context.Set<Lesson>()
                    .Include(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                    .Where(l => l.Section.Classroom.InstructorId == userId)
                    .ToListAsync();
                ViewBag.Lessons = new SelectList(lessons, "Id", "Title");
                return View(model);
            }

            quiz.Title = model.Title;
            quiz.Description = model.Description;
            quiz.PassingScore = model.PassingScore;

            _context.Set<Quiz>().Update(quiz);
            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật Quiz thành công";
            return RedirectToAction(nameof(Index));
        }

        // ==================== DELETE - Xóa Quiz ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Set<Quiz>()
                .Include(q => q.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (quiz.Lesson.Section.Classroom.InstructorId != userId)
                return Forbid();

            _context.Set<Quiz>().Remove(quiz);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa Quiz thành công";
            return RedirectToAction(nameof(Index));
        }

        // ==================== MANAGE QUESTIONS - Quản lý câu hỏi ====================
        public async Task<IActionResult> ManageQuestions(int quizId)
        {
            var quiz = await _context.Set<Quiz>()
                .Include(q => q.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (quiz.Lesson.Section.Classroom.InstructorId != userId)
                return Forbid();

            return View(quiz);
        }

        // ==================== ADD QUESTION ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(int quizId, QuizQuestion model)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(ManageQuestions), new { quizId });

            model.QuizId = quizId;
            var existingQuestions = await _context.Set<QuizQuestion>().Where(q => q.QuizId == quizId).ToListAsync();
            model.Order = existingQuestions.Count + 1;

            _context.Set<QuizQuestion>().Add(model);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đã thêm câu hỏi mới";
            return RedirectToAction(nameof(ManageQuestions), new { quizId });
        }


        // ==================== CẬP NHẬT CÂU HỎI (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuestion(int id, int quizId, string QuestionText, int Points)
        {
            var question = await _context.Set<QuizQuestion>().FindAsync(id);
            if (question == null) return NotFound();

            question.QuestionText = QuestionText;
            question.Points = Points;

            _context.Set<QuizQuestion>().Update(question);
            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật câu hỏi thành công";
            return RedirectToAction(nameof(ManageQuestions), new { quizId });
        }

        // ==================== DELETE QUESTION ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id, int quizId)
        {
            var question = await _context.Set<QuizQuestion>().FindAsync(id);
            if (question != null)
            {
                _context.Set<QuizQuestion>().Remove(question);
                await _context.SaveChangesAsync();
                TempData["success"] = "Đã xóa câu hỏi";
            }
            return RedirectToAction(nameof(ManageQuestions), new { quizId });
        }

        // ==================== ADD ANSWER ====================
        [HttpPost]
        public async Task<IActionResult> AddAnswer(int quizId, int questionId, QuizAnswer model)
        {
            model.QuestionId = questionId;
            var existingAnswers = await _context.Set<QuizAnswer>().Where(a => a.QuestionId == questionId).ToListAsync();
            model.Order = existingAnswers.Count + 1;

            _context.Set<QuizAnswer>().Add(model);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đã thêm đáp án";
            return RedirectToAction(nameof(ManageQuestions), new { quizId });
        }

        // ==================== CẬP NHẬT ĐÁP ÁN (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAnswer(int id, int quizId, string AnswerText, string AnswerLabel, bool IsCorrect)
        {
            var answer = await _context.Set<QuizAnswer>().Include(a => a.Question).FirstOrDefaultAsync(a => a.Id == id);
            if (answer == null) return NotFound();

            // Nếu đây là single choice và set đáp án này là đúng, thì bỏ Correct các đáp án khác cùng câu hỏi
            if (IsCorrect)
            {
                var otherAnswers = await _context.Set<QuizAnswer>()
                    .Where(a => a.QuestionId == answer.QuestionId && a.Id != id)
                    .ToListAsync();
                foreach (var oa in otherAnswers) oa.IsCorrect = false;
            }

            answer.AnswerText = AnswerText;
            answer.AnswerLabel = AnswerLabel;
            answer.IsCorrect = IsCorrect;

            _context.Set<QuizAnswer>().Update(answer);
            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật đáp án thành công";
            return RedirectToAction(nameof(ManageQuestions), new { quizId });
        }

        // ==================== DELETE ANSWER ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int id, int quizId)
        {
            var answer = await _context.Set<QuizAnswer>().FindAsync(id);
            if (answer != null)
            {
                _context.Set<QuizAnswer>().Remove(answer);
                await _context.SaveChangesAsync();
                TempData["success"] = "Đã xóa đáp án";
            }
            return RedirectToAction(nameof(ManageQuestions), new { quizId });
        }

        // ==================== RESULTS - Xem kết quả học sinh ====================
        public async Task<IActionResult> Results(int quizId)
        {
            var quiz = await _context.Set<Quiz>()
                .Include(q => q.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Classroom)
                .Include(q => q.StudentAttempts)
                    .ThenInclude(sa => sa.Student)
                .Include(q => q.StudentAttempts)
                    .ThenInclude(sa => sa.Answers)
                        .ThenInclude(sqa => sqa.Question)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (quiz.Lesson.Section.Classroom.InstructorId != userId)
                return Forbid();

            return View(quiz);
        }

        // INPUT MODELS
        public class QuizAnswerInput
        {
            public string AnswerText { get; set; } = string.Empty;
            public string AnswerLabel { get; set; } = "A";
            public bool IsCorrect { get; set; }
        }

        // ==================== LẤY DATA ĐỂ EDIT (API) ====================
        [HttpGet]
        public async Task<IActionResult> GetQuestion(int id)
        {
            var question = await _context.Set<QuizQuestion>().FindAsync(id);
            if (question == null) return NotFound();
            return Json(new { id = question.Id, text = question.QuestionText, points = question.Points });
        }

        [HttpGet]
        public async Task<IActionResult> GetAnswer(int id)
        {
            var answer = await _context.Set<QuizAnswer>().FindAsync(id);
            if (answer == null) return NotFound();
            return Json(new { id = answer.Id, text = answer.AnswerText, label = answer.AnswerLabel, isCorrect = answer.IsCorrect });
        }
    }
}