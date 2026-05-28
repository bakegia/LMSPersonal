using LMSfinal.Models.EF;

namespace LMSfinal.Models.ViewModels.Student
{
    public class NotificationListViewModel
    {
        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}