using Microsoft.AspNetCore.SignalR;

namespace LMSfinal.Hubs
{
    public sealed class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
