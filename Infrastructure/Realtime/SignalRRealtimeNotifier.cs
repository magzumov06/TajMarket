using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Realtime;

public class SignalRRealtimeNotifier(IHubContext<CourierHub> hubContext) : IRealtimeNotifier
{
    public async Task BroadcastCourierUpdateAsync(CourierLiveUpdatePayload payload)
    {
        await hubContext.Clients.All.SendAsync("CourierUpdated", payload);
    }
}