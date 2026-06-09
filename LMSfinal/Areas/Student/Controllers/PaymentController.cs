using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.Enums;
using LMSfinal.Models.Momo;
using LMSfinal.Models.ViewModels.Student;
using LMSfinal.Models.Vnpay;
using LMSfinal.Services.Momo;
using LMSfinal.Services.Vnpay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Security.Claims;

namespace LMSfinal.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMomoService momoService, IVnPayService vnPayService)
        {
            _context = context;
            _userManager = userManager;
            _momoService = momoService;
            _vnPayService = vnPayService;
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
        [Authorize]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)

        {

            var response = await _momoService.CreatePaymentMomo(model);
            if (string.IsNullOrEmpty(response.PayUrl))
            {
                return Content("Lỗi: Không nhận được PayUrl từ Momo API.");
            }

            return Redirect(response.PayUrl);

        }
        public async Task<IActionResult> PaymentCallBack()

        {
            var requestQuery = HttpContext.Request.Query;
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

            if (requestQuery["resultCode"] != "0")
            {
                TempData["error"] = "Thanh toán qua Momo bị hủy hoặc thất bại!";
                return RedirectToAction(nameof(Index));
            }

            // Thanh toán THÀNH CÔNG: Lấy PaymentId từ orderId (Tách chuỗi vì ở dưới ta ghép "ID_Tick")
            string moOrderId = requestQuery["orderId"].ToString();
            string[] parts = moOrderId.Split('_');
            
            if (parts.Length > 0 && int.TryParse(parts[0], out int paymentId))
            {
                // Xác thực số tiền
                var paymentDb = await _context.ClassroomPayments.FindAsync(paymentId);
                var paidAmount = Convert.ToDecimal(requestQuery["amount"]);

                if (paymentDb != null && paymentDb.Amount == paidAmount)
                {
                    await Checkout(paymentId);
                    TempData["success"] = "Thanh toán học phí thành công!";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task Checkout(int paymentId)
        {
            // 1. Tìm khoản thanh toán đang thực hiện dựa vào ID
            var payment = await _context.ClassroomPayments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return;

            // 2. Chuyển trạng thái thanh toán
            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.Now;
            payment.UpdatedAt = DateTime.Now;

            // 3. Mở khóa lớp học cho sinh viên nếu đang bị khóa
            var enrollment = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == payment.ClassroomId && cs.StudentId == payment.StudentId);

            if (enrollment != null && enrollment.IsLocked)
            {
                enrollment.IsLocked = false;
                enrollment.LockedAt = null;
                enrollment.LockReason = null;
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePaymentMomo(int paymentId)
        {
            var userId = _userManager.GetUserId(User);
            var payment = await _context.ClassroomPayments
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.StudentId == userId && p.Status != PaymentStatus.Paid);

            if (payment == null) return NotFound("Phiếu thanh toán không hợp lệ.");

            // Ép chuỗi orderInfo về không dấu/đơn giản để không bị lỗi chữ ký Momo
            var model = new OrderInfo
            {
                OrderId = $"{payment.Id}_{DateTime.Now.Ticks}", // CHỐNG TRÙNG LẶP CHO MOMO (Lấy PaymentId nối với Tick)
                Amount = (double)payment.Amount,
                FullName = "Hoc Sinh",
                OrderInformation = $"Thanh toan hoc phi cho lop {payment.ClassroomId}" 
            };

            var response = await _momoService.CreatePaymentMomo(model);
            
            // Xử lý chống Null nếu Momo từ chối
            if (response == null || string.IsNullOrEmpty(response.PayUrl))
            {
                TempData["error"] = $"Lỗi từ Momo API: {response?.Message ?? "Không xác định"}";
                return RedirectToAction(nameof(Index));
            }

            return Redirect(response.PayUrl);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrlVnpay(int paymentId)
        {
            var mssv = User.Claims.FirstOrDefault(c => c.Type == "MSSV")?.Value ?? "Unknown";
            var fullname = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Hoc Sinh";
            var userId = _userManager.GetUserId(User);
            var payment = await _context.ClassroomPayments
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.StudentId == userId && p.Status != PaymentStatus.Paid);

            if (payment == null) return NotFound("Phiếu thanh toán không hợp lệ.");

            var model = new PaymentInformationModel
            {
                OrderType = "other",
                Amount = payment.Amount,
                Name = $"{payment.Id}_{DateTime.Now.Ticks}", // CHỐNG TRÙNG LẶP CHO VNPAY
                OrderDescription = $"Thanh toan hoc phi cho lop {payment.ClassroomId} - Sinh vien: {fullname} - MSSV: {mssv}"
            };

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(HttpContext.Request.Query);

            // Kiểm tra Vnpay trả về responseCode "00" (Giao dịch thành công)
            if (response == null || !response.Success || response.VnPayResponseCode != "00")
            {
                TempData["error"] = "Lỗi thanh toán qua VNPay hoặc giao dịch bị hủy.";
                return RedirectToAction(nameof(Index));
            }

            // OrderId được lấy từ trả về của Vnpay (đó chính là model.Name ta truyền đi)
            string vnOrderId = response.OrderId;
            string[] parts = vnOrderId.Split('_');

            if (parts.Length > 0 && int.TryParse(parts[0], out int paymentId))
            {

                // Bảo mật: Kiểm tra xem số tiền khách thanh toán qua Vnpay có đúng với DB gốc không
                var paymentDb = await _context.ClassroomPayments.FindAsync(paymentId);
                Console.WriteLine($"DB Amount: {paymentDb.Amount}");
                Console.WriteLine($"VNPay Amount: {response.Amount}");
                if (paymentDb != null && paymentDb.Amount == (decimal)response.Amount)
                {
                    await Checkout(paymentId);
                    TempData["success"] = "Thanh toán VNPay học phí thành công!";
                }
                else
                {
                    TempData["error"] = "Số tiền thanh toán không khớp với học phí.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}