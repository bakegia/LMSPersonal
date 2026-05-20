using Microsoft.AspNetCore.Mvc;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
