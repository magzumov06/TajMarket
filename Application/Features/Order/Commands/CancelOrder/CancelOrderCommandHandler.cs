using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.CancelOrder;

public class CancelOrderCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IRealtimeNotifier realtimeNotifier,
    ILogger<CancelOrderCommandHandler> logger)
    : IRequestHandler<CancelOrderCommand, Response<string>>
{
    public async Task<Response<string>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var (userId, orderId) = (request.UserId, request.OrderId);

        try
        {
            logger.LogInformation("CancelOrderAsync started for order {OrderId} and user {UserId}", orderId, userId);

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("Order {OrderId} not found for user {UserId}", orderId, userId);
                return new Response<string>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }

            if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            {
                logger.LogWarning("Order {OrderId} cannot be cancelled because status is {Status}", orderId, order.Status);
                return new Response<string>(HttpStatusCode.BadRequest, "Ин фармоиш дигар бекор карда намешавад");
            }

            await using var transaction = await context.BeginTransactionAsync(cancellationToken);

            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var variant = await context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId.Value, cancellationToken);

                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;
                    }
                    else
                    {
                        var product = await context.Products
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                        if (product != null)
                            product.StockQuantity += item.Quantity;
                    }
                }
                else
                {
                    var product = await context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                    if (product != null)
                        product.StockQuantity += item.Quantity;
                }
            }

            order.Status = OrderStatus.Cancelled;

            if (order.Payment != null)
            {
                if (order.Payment.Status == PaymentStatus.Completed)
                {
                    order.Payment.Status = PaymentStatus.Refunded;
                }
                else if (order.Payment.Status == PaymentStatus.Pending)
                {
                    order.Payment.Status = PaymentStatus.Failed;
                }
            }

            await OrderCourierHelper.ReleaseCourierIfAssignedAsync(context, realtimeNotifier, logger, order, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                userId,
                "Фармоиш бекор шуд",
                $"Фармоиши шумо №{order.OrderNumber} бекор карда шуд"), cancellationToken);

            logger.LogInformation("Order {OrderId} cancelled successfully", orderId);

            return new Response<string>(HttpStatusCode.OK, "Order successfully cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(e, "CancelOrderAsync failed for order {OrderId} and user {UserId}", orderId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}