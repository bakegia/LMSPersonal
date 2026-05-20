using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /categories
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Danh mục khóa học - LMS Academy";
            ViewData["MetaDescription"] = "Khám phá các danh mục khóa học được sắp xếp theo lĩnh vực. Tìm khóa học phù hợp với bạn.";
            ViewData["MetaKeywords"] = "danh mục khóa học, lĩnh vực học tập, phân loại khóa học";
            ViewData["MetaImage"] = "/images/og-categories.jpg";

            var categories = await _context.Categories
                .Include(c => c.Courses)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: /categories/{slug}
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return RedirectToAction("Index");

            var category = await _context.Categories
                .Include(c => c.Courses)
                .ThenInclude(c => c.Sections)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
                return NotFound();

            ViewData["Title"] = $"{category.Name} - LMS Academy";
            ViewData["MetaDescription"] = category.Description ?? $"Khóa học trong danh mục {category.Name}";
            ViewData["MetaKeywords"] = $"{category.Name}, khóa học";

            return View(category);
        }
    }
}