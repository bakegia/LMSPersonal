using System.Diagnostics;
using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "LMS Academy - Nền tảng học tập trực tuyến";
            ViewData["MetaDescription"] = "Khám phá hàng ngàn khóa học chất lượng cao từ các chuyên gia hàng đầu";
            ViewData["MetaKeywords"] = "học tập trực tuyến, khóa học, kỹ năng";

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Sections)
                .ToListAsync();

            var categories = await _context.Categories
                .Include(c => c.Courses)
                .ToListAsync();

            var homeData = new
            {
                Courses = courses,
                Categories = categories
            };

            return View(homeData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult NotFound()
        {
            return View();
        }
    }
}
