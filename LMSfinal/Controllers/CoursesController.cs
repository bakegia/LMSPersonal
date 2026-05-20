using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /courses
        public async Task<IActionResult> Index(int page = 1, string? categorySlug = null)
        {
            const int pageSize = 12;

            ViewData["Title"] = "T?t c? khóa h?c - LMS Academy";
            ViewData["MetaDescription"] = "Danh sách ð?y ð? các khóa h?c có s?n trên n?n t?ng";
            ViewData["MetaKeywords"] = "khóa h?c, l?p tr?nh, thi?t k?, kinh doanh";

            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Sections)
                .AsQueryable();

            if (!string.IsNullOrEmpty(categorySlug))
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Slug == categorySlug);

                if (category != null)
                {
                    query = query.Where(c => c.CategoryId == category.CategoryId);
                    ViewData["CurrentCategory"] = category;
                }
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var courses = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;

            return View(courses);
        }

        // GET: /courses/{slug}
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (course == null)
                return NotFound();

            ViewData["Title"] = $"{course.Title} - LMS Academy";
            ViewData["MetaDescription"] = course.Description ?? $"Khóa h?c {course.Title}";
            ViewData["MetaKeywords"] = $"{course.Title}, {course.Category?.Name}";

            return View(course);
        }
    }
}