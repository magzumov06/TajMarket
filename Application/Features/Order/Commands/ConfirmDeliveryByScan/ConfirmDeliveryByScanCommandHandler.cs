using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.DTOs;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.ConfirmDeliveryByScan;

public class ConfirmDeliveryByScanCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IRealtimeNotifier realtimeNotifier,
    ILogger<ConfirmDeliveryByScanCommandHandler> logger)
    : IRequestHandler<ConfirmDeliveryByScanCommand, Response<OrderDetailDto>>
{
    public async Task<Response<OrderDetailDto>> Handle(ConfirmDeliveryByScanCommand request, CancellationToken cancellationToken)
    {
        var (courierUserId, dto) = (request.CourierUserId, request.Dto);

        try
        {
            logger.LogInformation("Confirming delivery by scan for courier user {CourierUserId}", courierUserId);

            if (string.IsNullOrWhiteSpace(dto.ScannedCode))
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Коди скан кардашуда холист");

            var courier = await context.Couriers
                .FirstOrDefaultAsync(c => c.UserId == courierUserId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {CourierUserId}", courierUserId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                    .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.DeliveryConfirmationCode == dto.ScannedCode, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("No order found for scanned code (courier {CourierUserId})", courierUserId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Коди QR нодуруст аст ё фармоиш ёфт нашуд");
            }

            if (order.CourierId != courier.Id)
            {
                logger.LogWarning(
                    "Courier {CourierId} tried to confirm order {OrderId} not assigned to them",
                    courier.Id, order.Id);
                return new Response<OrderDetailDto>(HttpStatusCode.Forbidden, "Ин фармоиш ба шумо таъин нашудааст");
            }

            if (order.Status != OrderStatus.Shipped)
            {
                logger.LogWarning("Order {OrderId} is not in Shipped status, current {Status}", order.Id, order.Status);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,
                    "Танҳо фармоиши дар роҳ буда бо скан тасдиқ карда мешавад");
            }

            order.Status = OrderStatus.Delivered;

            if (order.Payment is { Status: PaymentStatus.Pending })
            {
                order.Payment.Status = PaymentStatus.Completed;
                order.Payment.PaidAt = DateTime.UtcNow;
            }

            await OrderCourierHelper.ReleaseCourierIfAssignedAsync(context, realtimeNotifier, logger, order, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                order.UserId,
                "Фармоиш расонида шуд",
                $"Фармоиши №{order.OrderNumber} ба шумо расонида шуд. Лутфан қабулро тасдиқ кунед.",
                alsoSms: true), cancellationToken);

            logger.LogInformation("Order {OrderId} confirmed as delivered via QR scan by courier {CourierId}", order.Id, courier.Id);

            return new Response<OrderDetailDto>(OrderMapper.ToDetailDto(order), "Супоридан тасдиқ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error confirming delivery by scan for courier user {CourierUserId}", courierUserId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}