using ClosedXML.Excel;
using LMSfinal.Data;
using LMSfinal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImportAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ImportAccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel");
                return RedirectToAction(
    "Index",
    "Account",
    new { area = "Admin" }
);
            }

            var errors = new List<string>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int rowNumber = 2; // bắt đầu từ dòng thứ 2 vì dòng 1 là header

            foreach (var row in rows)
            {
                try
                {
                    var userName = row.Cell(1).GetString()?.Trim();
                    var email = row.Cell(2).GetString()?.Trim();
                    var password = row.Cell(3).GetString()?.Trim();
                    var role = row.Cell(4).GetString()?.Trim();

                    // Validate required
                    if (string.IsNullOrWhiteSpace(userName))
                        errors.Add($"Dòng {rowNumber}: Username không được để trống");

                    if (string.IsNullOrWhiteSpace(email))
                        errors.Add($"Dòng {rowNumber}: Email không được để trống");

                    if (string.IsNullOrWhiteSpace(password))
                        errors.Add($"Dòng {rowNumber}: Password không được để trống");

                    if (string.IsNullOrWhiteSpace(role))
                        errors.Add($"Dòng {rowNumber}: Role không được để trống");

                    // Có lỗi thì bỏ qua dòng hiện tại
                    if (errors.Any(x => x.Contains($"Dòng {rowNumber}:")))
                    {
                        rowNumber++;
                        continue;
                    }

                    // Validate email
                    if (!new EmailAddressAttribute().IsValid(email))
                    {
                        errors.Add($"Dòng {rowNumber}: Email không đúng định dạng");
                        rowNumber++;
                        continue;
                    }

                    // Check user tồn tại
                    var existUser = await _userManager.FindByEmailAsync(email);

                    if (existUser != null)
                    {
                        errors.Add($"Dòng {rowNumber}: Email {email} đã tồn tại");
                        rowNumber++;
                        continue;
                    }

                    // Tạo user
                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            errors.Add($"Dòng {rowNumber}: {error.Description}");
                        }

                        rowNumber++;
                        continue;
                    }

                    // Tạo role nếu chưa tồn tại
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }

                    // Gán role
                    await _userManager.AddToRoleAsync(user, role);
                }
                catch (Exception ex)
                {
                    errors.Add($"Dòng {rowNumber}: {ex.Message}");
                }

                rowNumber++;
            }

            // Nếu có lỗi thì hiển thị
            if (errors.Any())
            {
                ViewBag.Errors = errors;
                return RedirectToAction(
                    "Index",
                    "Account",
                    new { area = "Admin" }
                );
            }

            TempData["Success"] = "Import thành công";
            return RedirectToAction(
    "Index",
    "Account",
    new { area = "Admin" }
);
        }
    }
}