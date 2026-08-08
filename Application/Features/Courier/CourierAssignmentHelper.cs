using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier;

internal static class CourierAssignmentHelper
{
    public static async Task<(Domain.Entities.OrderEntity.Order? Order, Response<string>? Error)> ValidateOrderForAssignmentAsync(
        IApplicationDbContext context, int orderId, ILogger logger, bool includeAddress, CancellationToken cancellationToken)
    {
        var query = context.Orders.AsQueryable();

        if (includeAddress)
            query = query.Include(o => o.ShippingAddress);

        var order = await query.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            logger.LogWarning("Order not found {OrderId}", orderId);
            return (null, new Response<string>(System.Net.HttpStatusCode.NotFound, "Фармоиш ёфт нашуд"));
        }

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered or OrderStatus.Returned)
        {
            logger.LogWarning("Order {OrderId} cannot be assigned because status is {Status}", orderId, order.Status);
            return (null, new Response<string>(System.Net.HttpStatusCode.BadRequest, "Ба ин фармоиш курьер таъин кардан мумкин нест"));
        }

        if (order.CourierId.HasValue)
        {
            logger.LogWarning("Order {OrderId} already has a courier assigned", orderId);
            return (null, new Response<string>(System.Net.HttpStatusCode.Conflict, "Ба ин фармоиш аллакай курьер таъин шудааст"));
        }

        return (order, null);
    }

    public static async Task PerformAssignmentAsync(
        IApplicationDbContext context, IMediator mediator, IRealtimeNotifier realtimeNotifier,
        Domain.Entities.OrderEntity.Order order, Domain.Entities.UserEntity.Courier courier, CancellationToken cancellationToken)
    {
        order.CourierId = courier.Id;
        courier.Status = CourierStatus.Assigned;

        await context.SaveChangesAsync(cancellationToken);

        await mediator.Send(new SendNotificationCommand(
            courier.UserId,
            "Фармоиши нав",
            $"Ба шумо фармоиши №{order.OrderNumber} таъин карда шуд"), cancellationToken);

        await realtimeNotifier.BroadcastCourierUpdateAsync(new CourierLiveUpdatePayload(
            courier.Id,
            courier.User?.FullName ?? string.Empty,
            courier.Latitude,
            courier.Longitude,
            courier.Status.ToString()));
    }
}