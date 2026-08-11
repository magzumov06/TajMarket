using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Realtime;

public class SignalRRealtimeNotifier(
    IHubContext<CourierHub> courierHub,
    IHubContext<NotificationHub> notificationHub) : IRealtimeNotifier
{
    public async Task BroadcastCourierUpdateAsync(CourierLiveUpdatePayload payload)
    {
        await courierHub.Clients.All.SendAsync("CourierUpdated", payload);
    }

    public async Task NotifyUserAsync(int userId, NotificationPayload notification)
    {
        await notificationHub.Clients.User(userId.ToString()).SendAsync("NotificationReceived", notification);
    }
    
    public async Task BroadcastNotificationAsync(NotificationPayload notification)
    {
        await notificationHub.Clients.All.SendAsync("NotificationReceived", notification);
    }
}