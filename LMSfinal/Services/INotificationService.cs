using LMSfinal.Data;
using LMSfinal.Hubs;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Services
{
    public interface INotificationService
    {
        Task CreateAsync(Notification notification);
        Task CreateManyAsync(IEnumerable<string> userIds, string title, string message, string type, string? entityType = null, int? entityId = null);
        Task CreateManyAsync(IEnumerable<Notification> notifications);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int take = 100);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int id, string userId);
        Task<int> MarkAllAsReadAsync(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            await SendRealtimeNotificationAsync(notification);
        }

        public async Task CreateManyAsync(IEnumerable<string> userIds, string title, string message, string type, string? entityType = null, int? entityId = null)
        {
            var notifications = userIds.Select(userId => new Notification
            {
                RecipientUserId = userId,
                Title = title,
                Message = message,
                Type = type,
                EntityType = entityType,
                EntityId = entityId,
                CreatedAt = DateTime.Now
            }).ToList();

            if (notifications.Count == 0)
                return;

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
            await SendRealtimeNotificationsAsync(notifications);
        }

        public async Task CreateManyAsync(IEnumerable<Notification> notifications)
        {
            var notificationList = notifications?.ToList() ?? new List<Notification>();

            if (notificationList.Count == 0)
                return;

            foreach (var notification in notificationList.Where(notification => notification.CreatedAt == default))
            {
                notification.CreatedAt = DateTime.Now;
            }

            _context.Notifications.AddRange(notificationList);
            await _context.SaveChangesAsync();
            await SendRealtimeNotificationsAsync(notificationList);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int take = 100)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.RecipientUserId == userId);

            if (notification == null || notification.IsRead)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
                return 0;

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return unread.Count;
        }

        private Task SendRealtimeNotificationAsync(Notification notification)
        {
            if (string.IsNullOrWhiteSpace(notification.RecipientUserId))
                return Task.CompletedTask;

            return _hubContext.Clients.User(notification.RecipientUserId)
        .SendAsync("ReceiveNotification", new
        {
            notification.Id,
            notification.Title,
            notification.Message,
            CreatedAt = notification.CreatedAt.ToString("dd/MM/yyyy HH:mm")
        });
        }

        private async Task SendRealtimeNotificationsAsync(IEnumerable<Notification> notifications)
        {
            var sendTasks = notifications
        .Where(x => !string.IsNullOrWhiteSpace(x.RecipientUserId))
        .Select(x =>
            _hubContext.Clients.User(x.RecipientUserId)
            .SendAsync("ReceiveNotification", new
            {
                x.Id,
                x.Title,
                x.Message,
                CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            }));

            await Task.WhenAll(sendTasks);
        }
    }
}