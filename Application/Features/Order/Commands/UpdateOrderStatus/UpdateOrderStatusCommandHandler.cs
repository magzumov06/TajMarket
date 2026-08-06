using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.DTOs;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IRealtimeNotifier realtimeNotifier,
    ILogger<UpdateOrderStatusCommandHandler> logger)
    : IRequestHandler<UpdateOrderStatusCommand, Response<OrderDetailDto>>
{
    public async Task<Response<OrderDetailDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var (sellerUserId, orderId, dto) = (request.SellerUserId, request.OrderId, request.Dto);

        try
        {
            logger.LogInformation(
                "Updating status for order {OrderId} to {Status} by seller user {SellerUserId}",
                orderId, dto.Status, sellerUserId);

            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == sellerUserId, cancellationToken);

            if (sellerProfile == null)
            {
                logger.LogWarning("Seller profile not found for user {SellerUserId}", sellerUserId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Профили фурӯшанда ёфт нашуд");
            }

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("Order not found {OrderId}", orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }

            var ownsOrder = order.OrderItems.Any(oi => oi.Product.SellerProfileId == sellerProfile.Id);

            if (!ownsOrder)
            {
                logger.LogWarning("Seller user {SellerUserId} tried to update order {OrderId} they do not own", sellerUserId, orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.Forbidden, "Шумо ба ин фармоиш дастрасӣ надоред");
            }

            if (order.Status is OrderStatus.Cancelled or OrderStatus.Returned)
            {
                logger.LogWarning("Order {OrderId} status cannot be changed because current status is {Status}", orderId, order.Status);
                return new Response<OrderDetailDto>(HttpStatusCode.Conflict, "Ҳолати ин фармоиш дигар тағйирнопазир аст");
            }

            order.Status = dto.Status;

            if (dto.Status == OrderStatus.Delivered &&
                order.Payment is { Status: PaymentStatus.Pending })
            {
                order.Payment.Status = PaymentStatus.Completed;
                order.Payment.PaidAt = DateTime.UtcNow;
            }

            if (dto.Status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Returned)
            {
                await OrderCourierHelper.ReleaseCourierIfAssignedAsync(context, realtimeNotifier, logger, order, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                order.UserId,
                "Ҳолати фармоиш тағйир ёфт",
                $"Фармоиши №{order.OrderNumber} ҳоло дар ҳолати «{OrderMapper.StatusLabel(dto.Status)}» аст"), cancellationToken);

            logger.LogInformation("Order {OrderId} status updated successfully to {Status}", orderId, dto.Status);

            return new Response<OrderDetailDto>(OrderMapper.ToDetailDto(order));
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateStatusAsync failed for order {OrderId}", orderId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}