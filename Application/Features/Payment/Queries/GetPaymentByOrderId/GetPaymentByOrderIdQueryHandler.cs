using System.Net;
using Application.Common.Interfaces;
using Application.Features.Payment.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Payment.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetPaymentByOrderIdQueryHandler> logger)
    : IRequestHandler<GetPaymentByOrderIdQuery, Response<PaymentDto>>
{
    public async Task<Response<PaymentDto>> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var (userId, orderId) = (request.UserId, request.OrderId);

        try
        {
            logger.LogInformation("Retrieving payment for order {OrderId} user {UserId}", orderId, userId);

            var payment = await context.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Order.UserId == userId, cancellationToken);

            if (payment == null)
            {
                logger.LogWarning("Payment not found for order {OrderId}", orderId);
                return new Response<PaymentDto>(HttpStatusCode.NotFound, "Пардохт ёфт нашуд");
            }

            logger.LogInformation("Payment retrieved successfully {PaymentId}", payment.Id);

            return new Response<PaymentDto>(new PaymentDto(
                payment.Id, payment.Amount, payment.Method, payment.Status, payment.PaidAt));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving payment for order {OrderId}", orderId);
            return new Response<PaymentDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}