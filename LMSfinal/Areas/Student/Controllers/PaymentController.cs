using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.Enums;
using LMSfinal.Models.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var payments = await _context.ClassroomPayments
                .Include(p => p.Classroom)
                    .ThenInclude(c => c.Course)
                .Where(p => p.StudentId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new StudentPaymentListItemVM
                {
                    PaymentId = p.Id,
                    ClassroomId = p.ClassroomId,
                    ClassroomName = p.Classroom!.NameClass,
                    CourseTitle = p.Classroom!.Course.Title,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    Status = p.Status,
                    PaidAt = p.PaidAt
                })
                .ToListAsync();

            return View(payments);
        }

        public async Task<IActionResult> Locked(int classroomId)
        {
            var userId = _userManager.GetUserId(User);

            var data = await _context.ClassroomPayments
                .Include(p => p.Classroom)
                .Where(p => p.StudentId == userId && p.ClassroomId == classroomId)
                .Select(p => new StudentPaymentLockedViewModel
                {
                    ClassroomId = p.ClassroomId,
                    ClassroomName = p.Classroom!.NameClass,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    Status = p.Status,
                    IsLocked = _context.ClassroomStudents
                        .Any(cs => cs.StudentId == userId && cs.ClassroomId == classroomId && cs.IsLocked),
                    LockReason = _context.ClassroomStudents
                        .Where(cs => cs.StudentId == userId && cs.ClassroomId == classroomId)
                        .Select(cs => cs.LockReason)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return View(data);
        }
    }
}