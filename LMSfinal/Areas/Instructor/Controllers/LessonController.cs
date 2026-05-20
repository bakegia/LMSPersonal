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
        public IActionResult Create(int sectionId) // Hứng tham số từ URL
        {
            if (sectionId == 0) return BadRequest();

            ViewBag.SectionId = sectionId; // Truyền ra View để bỏ vào form ẩn
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                if (lesson.VideoUpload != null && lesson.VideoUpload.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + lesson.VideoUpload.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await lesson.VideoUpload.CopyToAsync(fileStream);
                    }
                    lesson.VideoUrl = "/uploads/" + uniqueFileName;
                }
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
                
            }
            return View(lesson);
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
        public async Task<IActionResult> Edit(int id, Lesson lesson)
        {
            if (id != lesson.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (lesson.VideoUpload != null && lesson.VideoUpload.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + lesson.VideoUpload.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await lesson.VideoUpload.CopyToAsync(fileStream);
                        }
                        lesson.VideoUrl = "/uploads/" + uniqueFileName;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading video: " + ex.Message);
                    return View(lesson);
                }
                 _context.Entry(lesson).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(lesson);
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
        private async Task<string> SaveVideo(IFormFile videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
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

            return "/uploads/" + uniqueFileName;
        }
    }
}
