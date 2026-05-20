using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class ProfilePersonal : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilePersonal(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy ID của người dùng đang đăng nhập
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return NotFound("Người dùng chưa đăng nhập hoặc không tồn tại.");
            }

            // 2. Tìm thông tin UserProfile tương ứng trong database
            var profile = await _context.UserProfiles
                .Include(u => u.User) // Load thông tin từ bảng ApplicationUser nếu cần
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (profile == null)
            {
                // Nếu chưa có profile, bạn có thể hướng người dùng đến trang tạo mới
                // hoặc khởi tạo một model trống với UserId đã có
                return View(new UserProfile { UserId = userId });
            }

            return View(profile);
        }
        
    }
}
