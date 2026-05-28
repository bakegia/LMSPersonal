using LMSfinal.Data;
using LMSfinal.Models.EF;
using LMSfinal.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Services
{
    public interface IClassroomAccessService
    {
        Task<bool> EnsureStudentAccessAsync(string studentId, int classroomId);
    }

    public class ClassroomAccessService : IClassroomAccessService
    {
        private readonly ApplicationDbContext _context;

        public ClassroomAccessService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EnsureStudentAccessAsync(string studentId, int classroomId)
        {
            var enrollment = await _context.ClassroomStudents
                .Include(cs => cs.Classroom)
                .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == studentId);

            if (enrollment == null)
                return false;

            var payment = await _context.ClassroomPayments
                .FirstOrDefaultAsync(p => p.ClassroomId == classroomId && p.StudentId == studentId);

            if (payment == null)
            {
                var dueDate = enrollment.Classroom?.RegistrationDeadline ?? enrollment.EnrolledAt.AddDays(7);

                payment = new ClassroomPayment
                {
                    ClassroomId = classroomId,
                    StudentId = studentId,
                    Amount = enrollment.TotalPrice,
                    DueDate = dueDate,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                _context.ClassroomPayments.Add(payment);
                await _context.SaveChangesAsync();
            }

            if (payment.Status != PaymentStatus.Paid && payment.DueDate < DateTime.Now)
            {
                payment.Status = PaymentStatus.Overdue;
                payment.UpdatedAt = DateTime.Now;

                if (!enrollment.IsLocked)
                {
                    enrollment.IsLocked = true;
                    enrollment.LockedAt = DateTime.Now;
                    enrollment.LockReason = "Quá hạn thanh toán";
                }

                await _context.SaveChangesAsync();
            }

            return !enrollment.IsLocked;
        }
    }
}