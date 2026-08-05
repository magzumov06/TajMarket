namespace Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    Task BroadcastCourierUpdateAsync(CourierLiveUpdatePayload payload);
}

public record CourierLiveUpdatePayload(
    int Id,
    string FullName,
    double? Latitude,
    double? Longitude,
    string Status);