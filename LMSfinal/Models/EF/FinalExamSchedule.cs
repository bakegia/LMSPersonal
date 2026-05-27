using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSfinal.Models.EF
{
    public class FinalExamSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassroomId { get; set; }

        [ForeignKey("ClassroomId")]
        public virtual Classroom? Classroom { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày thi")]
        [DataType(DataType.Date)]
        public DateTime ExamDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời lượng")]
        [Range(15, 300, ErrorMessage = "Thời lượng từ 15 đến 300 phút")]
        public int DurationInMinutes { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phòng thi")]
        [StringLength(100)]
        public string RoomName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ProctorName { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}