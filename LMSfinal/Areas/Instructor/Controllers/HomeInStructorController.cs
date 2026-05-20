using Microsoft.AspNetCore.Mvc;

namespace LMSfinal.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class HomeInStructorController : Controller
    {
       
        public IActionResult Index()
        {
            return View();
        }
    }
}
