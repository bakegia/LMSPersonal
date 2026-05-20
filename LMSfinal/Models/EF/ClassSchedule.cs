namespace LMSfinal.Models.EF
{
    public class ClassSchedule
    {
        //ca hoc
        public int Id { get; set; }

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public DayOfWeek DayOfWeek { get; set; } // Thứ mấy
    }
}
