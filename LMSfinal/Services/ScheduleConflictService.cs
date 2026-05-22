using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Services
{
    public interface IScheduleConflictService
    {
        /// <summary>
        /// Kiểm tra học sinh có trùng lịch học không
        /// </summary>
        Task<(bool HasConflict, string ConflictMessage)> CheckStudentScheduleConflict(
            string studentId, int newClassroomId);

        /// <summary>
        /// Kiểm tra giáo viên có trùng lịch dạy không
        /// </summary>
        Task<(bool HasConflict, string ConflictMessage)> CheckInstructorScheduleConflict(
            string instructorId, int newClassroomId);

        /// <summary>
        /// Kiểm tra xung đột thời gian giữa 2 time slot
        /// </summary>
        bool HasTimeOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2);
    }

    public class ScheduleConflictService : IScheduleConflictService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleConflictService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kiểm tra học sinh có trùng lịch học không
        /// </summary>
        public async Task<(bool HasConflict, string ConflictMessage)> CheckStudentScheduleConflict(
            string studentId, int newClassroomId)
        {
            // Lấy lớp muốn đăng ký
            var newClassroom = await _context.Classrooms
                .Include(c => c.ClassSchedules)
                .ThenInclude(cs => cs.TimeSlot)
                .FirstOrDefaultAsync(c => c.Id == newClassroomId);

            if (newClassroom == null)
                return (true, "Lớp không tồn tại.");

            // Lấy thời khóa biểu của lớp muốn đăng ký
            var newSchedules = newClassroom.ClassSchedules.ToList();
            if (!newSchedules.Any())
                return (false, ""); // Lớp chưa có thời khóa biểu

            // Lấy danh sách lớp đã đăng ký của học sinh
            var enrolledClassrooms = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == studentId)
                .Include(cs => cs.Classroom)
                .ThenInclude(c => c.ClassSchedules)
                .ThenInclude(cs => cs.TimeSlot)
                .Select(cs => cs.Classroom)
                .ToListAsync();

            // So sánh từng cặp thời khóa biểu
            foreach (var newSchedule in newSchedules)
            {
                foreach (var enrolledClassroom in enrolledClassrooms)
                {
                    foreach (var enrolledSchedule in enrolledClassroom.ClassSchedules)
                    {
                        // Kiểm tra: cùng ngày + cùng giờ?
                        if (newSchedule.DayOfWeek == enrolledSchedule.DayOfWeek &&
                            HasTimeOverlap(newSchedule.TimeSlot.StartTime, newSchedule.TimeSlot.EndTime,
                                          enrolledSchedule.TimeSlot.StartTime, enrolledSchedule.TimeSlot.EndTime))
                        {
                            string conflictMessage = 
                                $"Trùng lịch với lớp '{enrolledClassroom.NameClass}' " +
                                $"({GetDayName(enrolledSchedule.DayOfWeek)}, " +
                                $"{enrolledSchedule.TimeSlot.StartTime:HH\\:mm}-{enrolledSchedule.TimeSlot.EndTime:HH\\:mm})";
                            
                            return (true, conflictMessage);
                        }
                    }
                }
            }

            return (false, "");
        }

        /// <summary>
        /// Kiểm tra giáo viên có trùng lịch dạy không
        /// </summary>
        public async Task<(bool HasConflict, string ConflictMessage)> CheckInstructorScheduleConflict(
            string instructorId, int newClassroomId)
        {
            // Lấy lớp muốn dạy
            var newClassroom = await _context.Classrooms
                .Include(c => c.ClassSchedules)
                .ThenInclude(cs => cs.TimeSlot)
                .FirstOrDefaultAsync(c => c.Id == newClassroomId);

            if (newClassroom == null)
                return (true, "Lớp không tồn tại.");

            // Lấy thời khóa biểu của lớp muốn dạy
            var newSchedules = newClassroom.ClassSchedules.ToList();
            if (!newSchedules.Any())
                return (false, ""); // Lớp chưa có thời khóa biểu

            // Lấy danh sách lớp giáo viên đang dạy
            var taughtClassrooms = await _context.Classrooms
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.ClassSchedules)
                .ThenInclude(cs => cs.TimeSlot)
                .ToListAsync();

            // So sánh từng cặp thời khóa biểu
            foreach (var newSchedule in newSchedules)
            {
                foreach (var taughtClassroom in taughtClassrooms)
                {
                    foreach (var taughtSchedule in taughtClassroom.ClassSchedules)
                    {
                        // Kiểm tra: cùng ngày + cùng giờ?
                        if (newSchedule.DayOfWeek == taughtSchedule.DayOfWeek &&
                            HasTimeOverlap(newSchedule.TimeSlot.StartTime, newSchedule.TimeSlot.EndTime,
                                          taughtSchedule.TimeSlot.StartTime, taughtSchedule.TimeSlot.EndTime))
                        {
                            string conflictMessage = 
                                $"Trùng lịch với lớp '{taughtClassroom.NameClass}' " +
                                $"({GetDayName(taughtSchedule.DayOfWeek)}, " +
                                $"{taughtSchedule.TimeSlot.StartTime:HH\\:mm}-{taughtSchedule.TimeSlot.EndTime:HH\\:mm})";
                            
                            return (true, conflictMessage);
                        }
                    }
                }
            }

            return (false, "");
        }

        /// <summary>
        /// Kiểm tra xung đột giữa 2 khoảng thời gian
        /// Công thức: start1 < end2 AND start2 < end1
        /// </summary>
        public bool HasTimeOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }

        /// <summary>
        /// Convert DayOfWeek enum sang tiếng Việt
        /// </summary>
        private string GetDayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ Nhật",
                _ => day.ToString()
            };
        }
    }
}

