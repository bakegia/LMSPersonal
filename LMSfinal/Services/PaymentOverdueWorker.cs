using LMSfinal.Data;
using LMSfinal.Models.EF;
using LMSfinal.Models.Enums;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Services
{
    public class PaymentOverdueWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentOverdueWorker> _logger;

        public PaymentOverdueWorker(IServiceScopeFactory scopeFactory, ILogger<PaymentOverdueWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessOverduePaymentsAsync(stoppingToken);
            }
        }

        private async Task ProcessOverduePaymentsAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var now = DateTime.Now;
                var reminderEnd = now.AddDays(1);

                var reminderPayments = await context.ClassroomPayments
                    .Include(p => p.Classroom)
                        .ThenInclude(c => c.Course)
                    .Include(p => p.Student)
                    .Where(p => p.Status != PaymentStatus.Paid
                                && p.DueDate >= now
                                && p.DueDate <= reminderEnd
                                && p.ReminderSentAt == null)
                    .ToListAsync(stoppingToken);

                foreach (var payment in reminderPayments)
                {
                    if (payment.Student?.Email == null || payment.Classroom == null)
                    {
                        payment.ReminderSentAt = now;
                        continue;
                    }

                    var subject = "Nhắc đóng học phí - sắp đến hạn";
                    var className = payment.Classroom.NameClass;
                    var courseTitle = payment.Classroom.Course?.Title ?? className;
                    var amountText = payment.Amount.ToString("N0");

                    var body = $@"
                        <h3>Nhắc đóng học phí</h3>
                        <p>Lớp: <strong>{className}</strong></p>
                        <p>Môn: <strong>{courseTitle}</strong></p>
                        <p>Số tiền: <strong>{amountText} VNĐ</strong></p>
                        <p>Hạn thanh toán: <strong>{payment.DueDate:dd/MM/yyyy HH:mm}</strong></p>
                        <p>Vui lòng thanh toán đúng hạn để tránh bị khóa lớp.</p>";

                    try
                    {
                        await emailSender.SendEmailAsync(payment.Student.Email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Send reminder email failed for PaymentId {PaymentId}", payment.Id);
                    }

                    await notificationService.CreateAsync(new Notification
                    {
                        RecipientUserId = payment.StudentId,
                        Title = "Nhắc đóng học phí",
                        Message = $"Lớp {className} sắp đến hạn thanh toán ({payment.DueDate:dd/MM/yyyy HH:mm}).",
                        Type = "PaymentReminder",
                        EntityType = "ClassroomPayment",
                        EntityId = payment.Id,
                        CreatedAt = now
                    });

                    payment.ReminderSentAt = now;
                    payment.UpdatedAt = now;
                }

                var overduePayments = await context.ClassroomPayments
                    .Where(p => p.Status != PaymentStatus.Paid && p.DueDate < now)
                    .ToListAsync(stoppingToken);

                foreach (var payment in overduePayments)
                {
                    payment.Status = PaymentStatus.Overdue;
                    payment.UpdatedAt = now;

                    var enrollment = await context.ClassroomStudents
                        .FirstOrDefaultAsync(
                            cs => cs.ClassroomId == payment.ClassroomId && cs.StudentId == payment.StudentId,
                            stoppingToken);

                    if (enrollment != null && !enrollment.IsLocked)
                    {
                        enrollment.IsLocked = true;
                        enrollment.LockedAt = now;
                        enrollment.LockReason = "Quá hạn thanh toán";
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PaymentOverdueWorker failed.");
            }
        }
    }
}
