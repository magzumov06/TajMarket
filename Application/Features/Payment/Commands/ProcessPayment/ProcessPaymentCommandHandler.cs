using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.DTOs;
using Application.Features.Order.Queries.GetOrderDetail;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Payment.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IStripePaymentService stripePaymentService,
    ILogger<ProcessPaymentCommandHandler> logger)
    : IRequestHandler<ProcessPaymentCommand, Response<OrderDetailDto>>
{
    public async Task<Response<OrderDetailDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("Processing payment for user {UserId} order {OrderId}", userId, dto.OrderId);

            var payment = await context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == dto.OrderId && p.Order.UserId == userId, cancellationToken);

            if (payment == null)
            {
                logger.LogWarning("Payment not found for order {OrderId} user {UserId}", dto.OrderId, userId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                logger.LogWarning("Payment already completed for order {OrderId}", dto.OrderId);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Ин фармоиш аллакай пардохта шудааст");
            }

            if (payment.Order.Status == OrderStatus.Cancelled)
            {
                logger.LogWarning("Payment rejected because order cancelled {OrderId}", dto.OrderId);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Ин фармоиш бекор карда шудааст");
            }

            if (dto.Method == PaymentMethod.Cash)
            {
                logger.LogWarning("Cash payment selected for order {OrderId}", dto.OrderId);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Пардохти нақдӣ ҳангоми супоридани фармоиш анҷом мешавад");
            }

            if (dto.Method == PaymentMethod.Card && string.IsNullOrWhiteSpace(dto.CardToken))
            {
                logger.LogWarning("Card token missing for order {OrderId}", dto.OrderId);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Маълумоти корт нопурра аст");
            }

            if (dto.Method == PaymentMethod.Stripe)
            {
                logger.LogInformation("Processing Stripe payment for order {OrderId}", dto.OrderId);

                if (string.IsNullOrWhiteSpace(dto.StripePaymentMethodId))
                    return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Stripe payment method ID is required");

                var createIntentResult = await stripePaymentService.CreatePaymentIntentAsync(
                    payment.Amount, payment.OrderId.ToString(), $"Order #{payment.Order.OrderNumber}");

                if (!createIntentResult.Success)
                {
                    logger.LogWarning("Stripe payment intent creation failed for order {OrderId}", dto.OrderId);
                    return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, createIntentResult.Message);
                }

                var paymentIntent = createIntentResult.Data;

                var confirmResult = await stripePaymentService.ConfirmPaymentIntentAsync(
                    paymentIntent.Id, dto.StripePaymentMethodId);

                if (!confirmResult.Success)
                {
                    logger.LogWarning("Stripe payment confirmation failed for order {OrderId}", dto.OrderId);
                    return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, confirmResult.Message);
                }

                payment.Method = PaymentMethod.Stripe;
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = paymentIntent.Id;
            }
            else
            {
                payment.Method = dto.Method;
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = Guid.NewGuid().ToString("N");
            }

            if (payment.Order.Status == OrderStatus.Pending)
                payment.Order.Status = OrderStatus.Confirmed;

            await context.SaveChangesAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                userId,
                "Пардохт анҷом ёфт",
                $"Пардохти фармоиши №{payment.Order.OrderNumber} ба маблағи {payment.Amount:0.00} сомонӣ бомуваффақият қабул шуд"), cancellationToken);

            logger.LogInformation("Payment completed successfully for order {OrderId}", payment.OrderId);

            return await mediator.Send(new GetOrderDetailQuery(payment.OrderId, userId), cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Payment processing failed for order {OrderId} user {UserId}", dto.OrderId, userId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}