using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.DTOs;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.ConfirmDeliveryByCode;


public class ConfirmDeliveryByCodeCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IRealtimeNotifier realtimeNotifier,
    ILogger<ConfirmDeliveryByCodeCommandHandler> logger)
    : IRequestHandler<ConfirmDeliveryByCodeCommand, Response<OrderDetailDto>>
{
    private const int MaxAttempts = 5;

    public async Task<Response<OrderDetailDto>> Handle(ConfirmDeliveryByCodeCommand request, CancellationToken cancellationToken)
    {
        var (courierUserId, orderId, dto) = (request.CourierUserId, request.OrderId, request.Dto);

        try
        {
            logger.LogInformation("Confirming delivery by code for order {OrderId}, courier user {CourierUserId}", orderId, courierUserId);

            if (string.IsNullOrWhiteSpace(dto.Code))
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Рамз холист");

            var courier = await context.Couriers
                .FirstOrDefaultAsync(c => c.UserId == courierUserId, cancellationToken);

            if (courier == null)
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier).ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");

            if (order.CourierId != courier.Id)
            {
                logger.LogWarning("Courier {CourierId} tried to confirm order {OrderId} not assigned to them", courier.Id, orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.Forbidden, "Ин фармоиш ба шумо таъин нашудааст");
            }

            if (order.Status != OrderStatus.Shipped)
            {
                logger.LogWarning("Order {OrderId} is not in Shipped status, current {Status}", orderId, order.Status);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,
                    "Танҳо фармоиши дар роҳ буда бо рамз тасдиқ карда мешавад");
            }

            if (string.IsNullOrEmpty(order.DeliveryConfirmationCode))
            {
                logger.LogError("Order {OrderId} is Shipped but has no confirmation code", orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Рамзи тасдиқ барои ин фармоиш мавҷуд нест");
            }

            if (order.DeliveryCodeAttempts >= MaxAttempts)
            {
                logger.LogWarning("Too many failed delivery code attempts for order {OrderId}", orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,
                    "Шумораи кӯшишҳо тамом шуд. Бо маъмурият тамос гиред");
            }

            if (dto.Code.Trim() != order.DeliveryConfirmationCode)
            {
                order.DeliveryCodeAttempts++;
                await context.SaveChangesAsync(cancellationToken);  
                logger.LogWarning("Wrong delivery code entered for order {OrderId}", orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,
                    $"Рамз нодуруст аст ({MaxAttempts - order.DeliveryCodeAttempts} кӯшиши боқимонда)");
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

            logger.LogInformation("Order {OrderId} confirmed as delivered via code by courier {CourierId}", orderId, courier.Id);

            return new Response<OrderDetailDto>(OrderMapper.ToDetailDto(order), "Супоридан тасдиқ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error confirming delivery by code for order {OrderId}", orderId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}