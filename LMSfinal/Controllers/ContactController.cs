using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMSfinal.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        public ContactController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: /contact
        public IActionResult Index()
        {
            ViewData["Title"] = "Liên hệ chúng tôi - LMS Academy";
            ViewData["MetaDescription"] = "Liên hệ với chúng tôi để được hỗ trợ hoặc có bất kỳ câu hỏi nào";
            ViewData["MetaKeywords"] = "liên hệ, hỗ trợ, khóa học";

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Send(string name, string email, string phone, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(message))
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng điền đầy đủ thông tin"
                });
            }

            try
            {
                // Lưu database
                var contact = new Contact
                {
                    FullName = name,
                    Email = email,
                    Phone = phone,
                    Subject = subject,
                    Message = message,
                    CreatedAt = DateTime.Now,
                    Status = "New"
                };

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                // =========================
                // 1. MAIL CẢM ƠN USER
                // =========================
                string customerSubject = "Cảm ơn bạn đã liên hệ LMS Academy";

                string customerBody = $@"
            <h2>Xin chào {name},</h2>

            <p>Cảm ơn bạn đã liên hệ với LMS Academy.</p>

            <p>Chúng tôi đã nhận được thông tin của bạn và sẽ phản hồi trong thời gian sớm nhất.</p>

            <hr />

            <h4>Thông tin bạn đã gửi:</h4>

            <p>
                <strong>Họ tên:</strong> {name}<br/>
                <strong>Email:</strong> {email}<br/>
                <strong>Số điện thoại:</strong> {phone}<br/>
                <strong>Chủ đề:</strong> {subject}<br/>
                <strong>Nội dung:</strong><br/>
                {message}
            </p>

            <br/>

            <p>Trân trọng,</p>
            <p><strong>LMS Academy</strong></p>
        ";

                await _emailSender.SendEmailAsync(email, customerSubject, customerBody);

                // =========================
                // 2. MAIL CHO ADMIN
                // =========================
                string adminEmail = "buiquyen@canhcam.com";

                string adminSubject = "Có liên hệ mới từ website LMS Academy";

                string adminBody = $@"
            <h2>Thông báo liên hệ mới</h2>

            <p>Một khách hàng vừa gửi liên hệ từ website.</p>

            <hr />

            <p>
                <strong>Họ tên:</strong> {name}<br/>
                <strong>Email:</strong> {email}<br/>
                <strong>Số điện thoại:</strong> {phone}<br/>
                <strong>Chủ đề:</strong> {subject}<br/>
                <strong>Nội dung:</strong><br/>
                {message}
            </p>
        ";

                await _emailSender.SendEmailAsync(adminEmail, adminSubject, adminBody);

                return Json(new
                {
                    success = true,
                    message = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm nhất!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}