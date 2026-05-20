using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using LMSfinal.Models.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            var model = new StudentProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                ProfileId = profile?.Id,
                Mssv = profile?.Mssv,
                DateOfBirth = profile?.DateOfBirth,
                Gender = profile?.Gender ?? string.Empty,
                PhoneNumber = profile?.PhoneNumber ?? user.PhoneNumber ?? string.Empty,
                Address = profile?.Address ?? string.Empty,
                AvatarUrl = profile?.ProfilePictureUrl ?? user.Avatar ?? string.Empty
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            var model = new StudentProfileEditInput
            {
                FullName = user.FullName ?? profile?.Fullname ?? string.Empty,
                Email = user.Email ?? profile?.Email ?? string.Empty,
                Mssv = profile?.Mssv ?? 0,
                DateOfBirth = profile?.DateOfBirth ?? DateTime.Today,
                Gender = profile?.Gender ?? string.Empty,
                PhoneNumber = profile?.PhoneNumber ?? user.PhoneNumber ?? string.Empty,
                Address = profile?.Address ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentProfileEditInput input)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            user.FullName = input.FullName;
            user.Email = input.Email;
            user.UserName = input.Email;
            user.PhoneNumber = input.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(input);
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id,
                    Name = input.FullName,
                    Fullname = input.FullName,
                    Email = input.Email,
                    PhoneNumber = input.PhoneNumber,
                    DateOfBirth = input.DateOfBirth,
                    Gender = input.Gender,
                    Address = input.Address,
                    Mssv = input.Mssv
                };
                _context.UserProfiles.Add(profile);
            }
            else
            {
                profile.Name = input.FullName;
                profile.Fullname = input.FullName;
                profile.Email = input.Email;
                profile.PhoneNumber = input.PhoneNumber;
                profile.DateOfBirth = input.DateOfBirth;
                profile.Gender = input.Gender;
                profile.Address = input.Address;
                profile.Mssv = input.Mssv;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
