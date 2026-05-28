using LMSfinal.Data;
using LMSfinal.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _context.ClassroomPayments
                .Include(p => p.Classroom)
                    .ThenInclude(c => c.Course)
                .Include(p => p.Student)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var payment = await _context.ClassroomPayments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                return NotFound();

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.Now;
            payment.UpdatedAt = DateTime.Now;

            var enrollment = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == payment.ClassroomId && cs.StudentId == payment.StudentId);

            if (enrollment != null && enrollment.IsLocked)
            {
                enrollment.IsLocked = false;
                enrollment.LockedAt = null;
                enrollment.LockReason = null;
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Đã xác nhận thanh toán.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockOverdue()
        {
            var now = DateTime.Now;

            var overduePayments = await _context.ClassroomPayments
                .Where(p => p.Status != PaymentStatus.Paid && p.DueDate < now)
                .ToListAsync();

            foreach (var payment in overduePayments)
            {
                payment.Status = PaymentStatus.Overdue;
                payment.UpdatedAt = now;

                var enrollment = await _context.ClassroomStudents
                    .FirstOrDefaultAsync(cs => cs.ClassroomId == payment.ClassroomId && cs.StudentId == payment.StudentId);

                if (enrollment != null && !enrollment.IsLocked)
                {
                    enrollment.IsLocked = true;
                    enrollment.LockedAt = now;
                    enrollment.LockReason = "Quá hạn thanh toán";
                }
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Đã khóa các học sinh quá hạn.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(int id)
        {
            var payment = await _context.ClassroomPayments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                return NotFound();

            var enrollment = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == payment.ClassroomId && cs.StudentId == payment.StudentId);

            if (enrollment != null)
            {
                enrollment.IsLocked = false;
                enrollment.LockedAt = null;
                enrollment.LockReason = null;
                await _context.SaveChangesAsync();
            }

            TempData["success"] = "Đã mở khóa lớp học.";
            return RedirectToAction(nameof(Index));
        }
    }
}
