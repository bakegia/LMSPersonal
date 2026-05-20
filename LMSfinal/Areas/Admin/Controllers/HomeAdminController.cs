using Microsoft.AspNetCore.Mvc;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeAdminController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.ChartData = new int[] { 10, 20, 15, 30, 25, 40, 50 };
            return View();
        }
    }
}
