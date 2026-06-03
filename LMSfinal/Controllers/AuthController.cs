using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.ViewModels;
using LMSfinal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace LMSfinal.Controllers
{
    public class AuthController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _DataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ITokenService _tokenService;
        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext _Context, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, ITokenService tokenService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _DataContext = _Context;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _tokenService = tokenService;
        }

        public IActionResult Index()
        {
            var schedule = new List<ScheduleItem>
    {
        new ScheduleItem
        {
            Day = DayOfWeek.Wednesday,
            PeriodStart = 1,
            PeriodEnd = 3,
            Subject = "Giáo dục thể chất",
            Room = "A23-01-GYM",
            Teacher = "Nguyễn Thị May"
        }
    };

            return View(schedule);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            // Tìm User bằng UserManager của Identity thông qua UserName người dùng nhập vào
            var user = await _userManager.FindByNameAsync(loginVM.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View(loginVM);
            }

            // Kiểm tra mật khẩu (tham số thứ 3 là lockoutOnFailure - cấu hình true nếu muốn khóa tài khoản khi nhập sai nhiều lần)
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginVM.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // 1. Tạo cặp Token (Access Token và Refresh Token)
                var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user);

                // 2. Nạp Access Token vào HttpOnly Cookie
                Response.Cookies.Append("X-Access-Token", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Sử dụng trong Production chạy HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(15) // Hết hạn sau 15 phút
                });

                // 3. Nạp Refresh Token vào HttpOnly Cookie độc lập
                Response.Cookies.Append("X-Refresh-Token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7) // Hết hạn sau 7 ngày
                });

                // Kiểm tra tính hợp lệ của đường dẫn chuyển hướng sau đăng nhập thành công
                if (!string.IsNullOrEmpty(loginVM.ReturnUrl) && Url.IsLocalUrl(loginVM.ReturnUrl))
                    return Redirect(loginVM.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa do nhập sai nhiều lần.");
            else if (result.IsNotAllowed)
                ModelState.AddModelError("", "Tài khoản chưa được phép đăng nhập (ví dụ: Chưa xác nhận Email).");
            else
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");

            return View(loginVM);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            // Xóa hoàn toàn các Cookie lưu trữ Token tại client
            Response.Cookies.Delete("X-Access-Token");
            Response.Cookies.Delete("X-Refresh-Token");

            return RedirectToAction("Login", "Auth,", new { area = "" });
        }
        //[AllowAnonymous]
        //public async Task<IActionResult> Login(string returnUrl)
        //{

        //    return View(new LoginVM { ReturnUrl = returnUrl });
        //}

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginVM loginVM)
        //{
        //    if (!ModelState.IsValid)
        //        return View(loginVM);

        //    var result = await _signInManager.PasswordSignInAsync(
        //        loginVM.UserName,
        //        loginVM.Password,
        //        false,
        //        false
        //    );

        //    if (result.Succeeded)
        //        return RedirectToAction("Index", "Home");

        //    if (result.IsLockedOut)
        //        ModelState.AddModelError("", "Tài khoản bị khóa");
        //    else if (result.IsNotAllowed)
        //        ModelState.AddModelError("", "Không được phép đăng nhập");
        //    else
        //        ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");

        //    return View(loginVM);
        //}
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // Generate OTP
                string otpCode = new Random().Next(100000, 999999).ToString();

                // Save OTP
                user.ResetCode = otpCode;
                user.ResetCodeExpiration = DateTime.Now.AddMinutes(5);

                await _userManager.UpdateAsync(user);

                // HTML Email Template
                string htmlBody = $@"
        <div style='background-color:#f4f6f9;padding:40px 0;font-family:Arial,sans-serif;'>

            <div style='max-width:600px;
                        margin:auto;
                        background:white;
                        border-radius:12px;
                        overflow:hidden;
                        box-shadow:0 4px 12px rgba(0,0,0,0.1);'>

                <!-- Header -->
                <div style='background:#2563eb;
                            padding:24px;
                            text-align:center;'>

                    <h1 style='color:white;
                               margin:0;
                               font-size:28px;'>
                        LMS System
                    </h1>

                </div>

                <!-- Content -->
                <div style='padding:40px;color:#333;'>

                    <h2 style='margin-top:0;color:#111827;'>
                        Quên mật khẩu
                    </h2>

                    <p style='font-size:16px;line-height:1.6;'>
                        Xin chào,
                    </p>

                    <p style='font-size:16px;line-height:1.6;'>
                        Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản LMS.
                    </p>

                    <p style='font-size:16px;line-height:1.6;'>
                        Vui lòng sử dụng mã OTP bên dưới:
                    </p>

                    <!-- OTP Box -->
                    <div style='text-align:center;margin:35px 0;'>

                        <span style='display:inline-block;
                                     background:#2563eb;
                                     color:white;
                                     padding:18px 36px;
                                     border-radius:10px;
                                     font-size:34px;
                                     font-weight:bold;
                                     letter-spacing:8px;'>

                            {otpCode}

                        </span>

                    </div>

                    <p style='font-size:15px;color:#dc2626;'>
                        Mã OTP sẽ hết hạn sau 5 phút.
                    </p>

                    <p style='font-size:15px;line-height:1.6;'>
                        Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.
                    </p>

                    <hr style='margin:30px 0;border:none;border-top:1px solid #e5e7eb;' />

                    <p style='text-align:center;
                              color:#9ca3af;
                              font-size:13px;'>

                        © 2026 LMS System. All rights reserved.

                    </p>

                </div>
            </div>
        </div>";

                // Send Email
                await _emailSender.SendEmailAsync(
                    email,
                    "Xác nhận quên mật khẩu",
                    htmlBody
                );
            }

            return RedirectToAction("ConfirmOtp", new { email });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordManual(ResetPasswordManualVM model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || user.ResetCode != model.Code || user.ResetCodeExpiration < DateTime.Now)
            {
                ModelState.AddModelError("", "Mã xác nhận không đúng hoặc đã hết hạn.");
                return View(model);
            }

            // 1. Xóa mật khẩu cũ và thiết lập mật khẩu mới
            await _userManager.RemovePasswordAsync(user); // Xóa pass hiện tại
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword); // Thêm pass mới

            if (result.Succeeded)
            {
                // 2. Xóa mã xác nhận sau khi dùng xong để bảo mật
                user.ResetCode = null;
                user.ResetCodeExpiration = null;
                await _userManager.UpdateAsync(user);

                return RedirectToAction("Login");
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ConfirmOtp(string email)
        {
            var model = new ResetPasswordManualVM
            {
                Email = email
            };

            return View(model);
        }
    }
}
