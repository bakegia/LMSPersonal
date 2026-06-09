using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class LessonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public LessonController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> Index()
        {
            var lessons = await _context.Lessons.ToListAsync();
            return View(lessons);
        }
        [HttpGet]
        public IActionResult Create(int sectionId)
        {
            if (sectionId == 0) return BadRequest();

            ViewBag.SectionId = sectionId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                return View(lesson);
            }

            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == lesson.SectionId);

            if (section == null)
            {
                return NotFound();
            }

            if (lesson.VideoUpload != null && lesson.VideoUpload.Length > 0)
            {
                lesson.VideoUrl = await SaveVideo(lesson.VideoUpload);
            }

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction("Section", "Instructor", new { area = "Instructor", classroomId = section.ClassroomId });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return View("NotFound");
            }
            return View(lesson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lesson lesson)
        {
            if (id != lesson.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(lesson);
            }

            var existingLesson = await _context.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (existingLesson == null)
            {
                return NotFound();
            }

            existingLesson.Title = lesson.Title;
            existingLesson.Slug = lesson.Slug;
            existingLesson.Summary = lesson.Summary;
            existingLesson.Content = lesson.Content;
            existingLesson.Order = lesson.Order;
            existingLesson.VideoUrl = existingLesson.VideoUrl;
            existingLesson.IsPreviewFree = lesson.IsPreviewFree;

            existingLesson.VideoDurationSeconds = lesson.VideoDurationSeconds;
            existingLesson.RequiredWatchPercent = lesson.RequiredWatchPercent;
            existingLesson.RequireQuiz = lesson.RequireQuiz;
            existingLesson.RequireQuizPass = lesson.RequireQuizPass;

            if (lesson.VideoUpload != null && lesson.VideoUpload.Length > 0)
            {
                existingLesson.VideoUrl = await SaveVideo(lesson.VideoUpload);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Section", new { area = "Instructor", classroomId = existingLesson.Section?.ClassroomId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null) return NotFound();
            return View(lesson);
        }
        private async Task<string?> SaveVideo(IFormFile videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "videos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + videoFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(fileStream);
            }

            return "/videos/" + uniqueFileName;
        }
    }
}
