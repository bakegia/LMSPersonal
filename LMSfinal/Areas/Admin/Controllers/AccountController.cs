using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.DTOs;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels;
using LMSfinal.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _DataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccountService _accountService;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext _Context, RoleManager<IdentityRole> roleManager, IAccountService accountService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _DataContext = _Context;
            _roleManager = roleManager;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var result = new List<UserWithRoleVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserWithRoleVM
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(result);
        }

        // ==================== CREATE STUDENT ====================
        [HttpGet]
        public async Task<IActionResult> CreateStudentAccount()
        {
            var roles = await _roleManager.Roles
                .Where(r => r.Name == "Student")
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new CreateStudentVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudentAccount(CreateStudentVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Lấy role Student
            var studentRole = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Name == "Student");

            if (studentRole == null)
            {
                ModelState.AddModelError("", "Role Student không tồn tại");
                return View(model);
            }

            model.RoleId = studentRole.Id;

            var result = await _accountService.CreateStudentAccountAsync(
                model.UserName,
                model.Email,
                model.FullName,
                model.Password,
                model.RoleId,
                model.Mssv,
                model.PhoneNumber,
                model.DateOfBirth,
                model.Gender,
                model.Address ?? "" 
            );

            if (result.Success)
            {
                TempData["success"] = result.Message;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        // ==================== CREATE INSTRUCTOR ====================
        [HttpGet]
        public async Task<IActionResult> CreateInstructorAccount()
        {
            var roles = await _roleManager.Roles
                .Where(r => r.Name == "Instructor")
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new CreateInstructorVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInstructorAccount(CreateInstructorVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Lấy role Instructor
            var instructorRole = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Name == "Instructor");

            if (instructorRole == null)
            {
                ModelState.AddModelError("", "Role Instructor không tồn tại");
                return View(model);
            }

            model.RoleId = instructorRole.Id;

            var (success, message, user) = await _accountService.CreateInstructorAccountAsync(
                model.UserName,
                model.Email,
                model.FullName,
                model.Password,
                model.RoleId,
                model.PhoneNumber,
                model.Department,
                model.SpecializedSubject,
                model.EmployeeCode,
                model.HireDate
            );

            if (success)
            {
                TempData["success"] = message;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", message);
            return View(model);
        }

        // ==================== UPDATE PROFILE ====================
        public async Task<IActionResult> UpdateProfile(string userId)
        {
            var profile = await _DataContext.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
            }

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfile model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var profile = await _DataContext.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == model.UserId);

            if (profile == null)
            {
                profile = new UserProfile();
                _DataContext.UserProfiles.Add(profile);
            }

            profile.UserId = model.UserId;
            profile.Mssv = model.Mssv;
            profile.Name = model.Name;
            profile.Fullname = model.Fullname;
            profile.Email = model.Email;
            profile.PhoneNumber = model.PhoneNumber;
            profile.DateOfBirth = model.DateOfBirth;
            profile.Gender = model.Gender;
            profile.Address = model.Address;

            if (model.Imageupload != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.Imageupload.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await model.Imageupload.CopyToAsync(stream);

                profile.ProfilePictureUrl = "/images/avatars/" + fileName;
            }

            await _DataContext.SaveChangesAsync();

            return RedirectToAction("ProfileDetail", new { userId = profile.UserId });
        }

        public async Task<IActionResult> ProfileDetail(string userId)
        {
            var profile = await _DataContext.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false });

            user.IsActive = isActive;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToRoute(new { controller = "Auth", action = "Login", area = "" });
        }

        // ==================== LIST STUDENT ====================
        [HttpGet]
        public async Task<IActionResult> StudentList()
        {
            var users = await _userManager.Users
                .Where(u => u.UserName != User.Identity.Name)
                .ToListAsync();

            var result = new List<UserWithRoleVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Chỉ lấy Student
                if (roles.Contains("Student"))
                {
                    result.Add(new UserWithRoleVM
                    {
                        User = user,
                        Roles = roles
                    });
                }
            }

            return View(result);
        }

        // ==================== LIST INSTRUCTOR ====================
        [HttpGet]
        public async Task<IActionResult> InstructorList()
        {
            var users = await _userManager.Users
                .Where(u => u.UserName != User.Identity.Name)
                .ToListAsync();

            var result = new List<UserWithRoleVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Chỉ lấy Instructor
                if (roles.Contains("Instructor"))
                {
                    result.Add(new UserWithRoleVM
                    {
                        User = user,
                        Roles = roles
                    });
                }
            }

            return View(result);
        }

        // ==================== DELETE ACCOUNT (AJAX) ====================
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null)
                    return Json(new { success = false, message = "Người dùng không tồn tại" });

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return Json(new { success = true, message = "Xóa tài khoản thành công" });

                return Json(new { success = false, message = "Lỗi xóa tài khoản" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==================== INSTRUCTOR DETAIL ====================
        [HttpGet]
        public async Task<IActionResult> InstructorDetail(string userId)
        {
            var instructor = await _DataContext.Instructors
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (instructor == null)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    Response.StatusCode = 404;
                    return View("NotFound");
                }

                instructor = new LMSfinal.Models.EF.Instructor
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = "",
                    Department = "Chưa cập nhật",
                    SpecializedSubject = "Chưa cập nhật",
                    EmployeeCode = $"EMP_{Guid.NewGuid().ToString().Substring(0, 8)}",
                    HireDate = DateTime.Now,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    ProfileImageUrl = ""
                };

                _DataContext.Instructors.Add(instructor);
                await _DataContext.SaveChangesAsync();
            }

            // Map sang ViewModel
            var vm = new UpdateInstructorVM
            {
                Id = instructor.Id,
                UserId = instructor.UserId,
                FullName = instructor.FullName,
                Email = instructor.Email,
                PhoneNumber = instructor.PhoneNumber,
                Department = instructor.Department,
                SpecializedSubject = instructor.SpecializedSubject,
                EmployeeCode = instructor.EmployeeCode,
                HireDate = instructor.HireDate,
                IsActive = instructor.IsActive,
                ProfileImageUrl = instructor.ProfileImageUrl,
                CreatedAt = instructor.CreatedAt,
                UpdatedAt = instructor.UpdatedAt
            };

            return View(vm);
        }

        // ==================== UPDATE INSTRUCTOR PROFILE ====================
        [HttpGet]
        public async Task<IActionResult> UpdateInstructorProfile(string userId)
        {
            var instructor = await _DataContext.Instructors
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (instructor == null)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            // Map Instructor to UpdateInstructorVM
            var model = new UpdateInstructorVM
            {
                Id = instructor.Id,
                UserId = instructor.UserId,
                FullName = instructor.FullName,
                Email = instructor.Email,
                PhoneNumber = instructor.PhoneNumber,
                Department = instructor.Department,
                SpecializedSubject = instructor.SpecializedSubject,
                EmployeeCode = instructor.EmployeeCode,
                HireDate = instructor.HireDate,
                IsActive = instructor.IsActive,
                ProfileImageUrl = instructor.ProfileImageUrl,
                CreatedAt = instructor.CreatedAt,
                UpdatedAt = instructor.UpdatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInstructorProfile(UpdateInstructorVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var instructor = await _DataContext.Instructors
                .FirstOrDefaultAsync(x => x.UserId == model.UserId);

            if (instructor == null)
            {
                ModelState.AddModelError("", "Không tìm thấy giáo viên");
                return View(model);
            }

            // Update properties
            instructor.FullName = model.FullName;
            instructor.Email = model.Email;
            instructor.PhoneNumber = model.PhoneNumber;
            instructor.Department = model.Department;
            instructor.SpecializedSubject = model.SpecializedSubject;
            instructor.EmployeeCode = model.EmployeeCode;
            instructor.HireDate = model.HireDate;
            instructor.IsActive = model.IsActive;
            instructor.UpdatedAt = DateTime.Now;

            // Upload Avatar
            if (model.ImageUpload != null && model.ImageUpload.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(instructor.ProfileImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", instructor.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Save new image
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageUpload.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars", fileName);

                // Create directory if not exists
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }

                instructor.ProfileImageUrl = "/images/avatars/" + fileName;
            }

            try
            {
                _DataContext.Instructors.Update(instructor);
                await _DataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật thông tin giáo viên thành công";
                return RedirectToAction("InstructorDetail", new { userId = instructor.UserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi cập nhật: {ex.Message}");
                return View(model);
            }
        }
    }
}
