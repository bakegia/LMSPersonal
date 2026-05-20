using LMSfinal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Admin.Controllers
{
    public class About : Controller
    {
        private readonly ApplicationDbContext _context;
        public About(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var contact = await _context.Contacts.ToListAsync();
            return View(contact);
        }
    }
}
