using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.DTOs;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.CompleteOrder;

public class CompleteOrderCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IRealtimeNotifier realtimeNotifier,
    ILogger<CompleteOrderCommandHandler> logger)
    : IRequestHandler<CompleteOrderCommand, Response<OrderDetailDto>>
{
    public async Task<Response<OrderDetailDto>> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var (userId, orderId) = (request.UserId, request.OrderId);

        try
        {
            logger.LogInformation("CompleteOrderAsync started for order {OrderId} by user {UserId}", orderId, userId);

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("Order not found {OrderId} for user {UserId}", orderId, userId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }

            if (order.Status is not (OrderStatus.Shipped or OrderStatus.Delivered))
            {
                logger.LogWarning("Order {OrderId} cannot be completed, current status {Status}", orderId, order.Status);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,
                    "Фармоиш ҳанӯз фиристода нашудааст, тасдиқи қабул имконнопазир аст");
            }

            if (order.CustomerConfirmedAt.HasValue)
            {
                logger.LogWarning("Order {OrderId} already confirmed by customer", orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.Conflict,
                    "Шумо аллакай қабули ин фармоишро тасдиқ кардаед");
            }

            order.CustomerConfirmedAt = DateTime.UtcNow;

            if (order.Status != OrderStatus.Delivered)
            {
                order.Status = OrderStatus.Delivered;

                if (order.Payment is { Status: PaymentStatus.Pending })
                {
                    order.Payment.Status = PaymentStatus.Completed;
                    order.Payment.PaidAt = DateTime.UtcNow;
                }
            }

            await OrderCourierHelper.ReleaseCourierIfAssignedAsync(context, realtimeNotifier, logger, order, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                order.UserId,
                "Фармоиш анҷом ёфт",
                $"Шумо қабули фармоиши №{order.OrderNumber}-ро тасдиқ кардед. Ташаккур!"), cancellationToken);

            logger.LogInformation("Order {OrderId} completed/confirmed by customer {UserId}", orderId, userId);

            return new Response<OrderDetailDto>(OrderMapper.ToDetailDto(order));
        }
        catch (Exception e)
        {
            logger.LogError(e, "CompleteOrderAsync failed for order {OrderId}", orderId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}