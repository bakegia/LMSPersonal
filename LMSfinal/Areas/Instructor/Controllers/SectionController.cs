using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class SectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SectionController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index(int classroomId)
        {
            if (classroomId == 0) return BadRequest("ID lớp học không hợp lệ.");

            // Lấy danh sách Section
            var sections = await _context.Sections
                .Include(s => s.Lessons)
                .Where(s => s.ClassroomId == classroomId)
                .OrderBy(s => s.Order)
                .ToListAsync();

            ViewBag.ClassroomId = classroomId;
            return View(sections);
        }
        
        [HttpPost]
        [Area("Instructor")]
        public async Task<IActionResult> Create(Section section)
        {
            // Quan trọng: Gỡ bỏ kiểm tra đối tượng Classroom liên kết 
            // vì Form chỉ gửi ClassroomId (int), không gửi nguyên object Classroom.
            ModelState.Remove("Classroom");
            ModelState.Remove("Lessons");

            if (ModelState.IsValid)
            {
                _context.Sections.Add(section);
                await _context.SaveChangesAsync();

                // Sau khi lưu, quay lại trang danh sách chương của chính lớp đó
                return RedirectToAction("Index", new { classroomId = section.ClassroomId });
            }

            // Nếu lỗi, load lại dữ liệu để View không bị trống
            ViewBag.ClassroomId = section.ClassroomId;
            var sections = await _context.Sections
                .Include(s => s.Lessons)
                .Where(s => s.ClassroomId == section.ClassroomId)
                .ToListAsync();
            return View("Index", sections);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return View("NotFound");
            }
            return View(section);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, int order)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return NotFound();

            section.Title = title;
            section.Order = order; // Cập nhật thứ tự

            await _context.SaveChangesAsync();
            // Quay lại trang quản lý nội dung của lớp học đó
            return RedirectToAction("Index", new { classroomId = section.ClassroomId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Tìm chương cần xóa
            var section = await _context.Sections.FindAsync(id);

            if (section != null)
            {
                int cid = section.ClassroomId; // Lưu lại ID lớp để quay về

                // 2. Xóa các bài học (Lesson) thuộc chương này trước (nếu DB không tự xóa)
                var lessons = _context.Lessons.Where(l => l.SectionId == id);
                _context.Lessons.RemoveRange(lessons);

                // 3. Xóa chương
                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();

                // 4. QUAN TRỌNG: Quay lại trang Index kèm theo ID lớp học
                return RedirectToAction("Index", new { classroomId = cid });
            }

            return BadRequest("Không tìm thấy chương.");
        }
        public async Task<IActionResult> Details(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return View("NotFound");
            }
            return View(section);
        }
        
    }
}
