using DocumentFormat.OpenXml.Spreadsheet;
using LMSfinal.Data;
using LMSfinal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Instructor")]
[Authorize(Roles = "Instructor")]
public class FinalExamInstructorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FinalExamInstructorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var fullName = user.FullName ?? user.UserName;

        // Lấy những lịch thi mà giảng viên này được phân công (ProctorName)
        var myProctorSchedules = await _context.FinalExamSchedules
            .Include(f => f.Classroom)
                .ThenInclude(c => c.Course)
            .Where(f => f.ProctorName == fullName)
            .OrderBy(f => f.ExamDate)
            .ToListAsync();

        return View(myProctorSchedules);
    }
}