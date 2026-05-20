using Microsoft.AspNetCore.Mvc;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SectionssController : Controller
    {
        public IActionResult Indexx()
        {
            return View();
        }
    }
}
