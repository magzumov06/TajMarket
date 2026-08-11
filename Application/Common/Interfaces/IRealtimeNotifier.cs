namespace Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    Task BroadcastCourierUpdateAsync(CourierLiveUpdatePayload payload);
    
    Task NotifyUserAsync(int userId, NotificationPayload notification);

    Task BroadcastNotificationAsync(NotificationPayload notification);

}

public record CourierLiveUpdatePayload(
    int Id,
    string FullName,
    double? Latitude,
    double? Longitude,
    string Status);
    
public record NotificationPayload(
    int Id, 
    string Title, 
    string Message,
    bool IsRead,
    DateTime CreatedAt);