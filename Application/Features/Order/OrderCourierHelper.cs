using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order;

internal static class OrderCourierHelper
{
    public static async Task ReleaseCourierIfAssignedAsync(
        IApplicationDbContext context,
        IRealtimeNotifier realtimeNotifier,
        ILogger logger,
        Domain.Entities.OrderEntity.Order order,
        CancellationToken cancellationToken)
    {
        if (!order.CourierId.HasValue)
            return;

        var courier = await context.Couriers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == order.CourierId.Value, cancellationToken);

        if (courier == null || courier.Status == CourierStatus.Offline)
            return;

        courier.Status = CourierStatus.Available;

        logger.LogInformation(
            "Courier {CourierId} released back to Available after order {OrderId} reached a terminal status",
            courier.Id, order.Id);

        await realtimeNotifier.BroadcastCourierUpdateAsync(new CourierLiveUpdatePayload(
            courier.Id,
            courier.User?.FullName ?? string.Empty,
            courier.Latitude,
            courier.Longitude,
            courier.Status.ToString()));
    }
}