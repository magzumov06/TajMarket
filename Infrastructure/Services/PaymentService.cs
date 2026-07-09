using System.Net;
using Domain.DTOs.OrderDto;
using Domain.DTOs.PaymentDtos;
using Domain.Entities.PaymentEntity;
using Domain.Enums;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PaymentService(
    DataContext context,
    IOrderService orderService,
    INotificationService notificationService,
    IStripePaymentService stripePaymentService) : IPaymentService
{
    #region Process

    public async Task<Response<OrderDetailDto>> ProcessAsync(int userId, ProcessPaymentDto dto)
    {
        try
        {
            var payment = await context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == dto.OrderId && p.Order.UserId == userId);

            if (payment == null)
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound,"Фармоиш ёфт нашуд");

            if (payment.Status == PaymentStatus.Completed)
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,"Ин фармоиш аллакай пардохта шудааст");

            if (payment.Order.Status == OrderStatus.Cancelled)
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,"Ин фармоиш бекор карда шудааст");

            if (dto.Method == PaymentMethod.Cash)
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,"Пардохти нақдӣ ҳангоми супоридани фармоиш анҷом мешавад");

            if (dto.Method == PaymentMethod.Card && string.IsNullOrWhiteSpace(dto.CardToken))
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,"Маълумоти корт нопурра аст");

            if (dto.Method == PaymentMethod.Stripe)
            {
                if (string.IsNullOrWhiteSpace(dto.StripePaymentMethodId))
                    return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,"Stripe payment method ID is required");

                // Create payment intent with Stripe
                var createIntentResult = await stripePaymentService.CreatePaymentIntentAsync(
                    payment.Amount,
                    payment.OrderId.ToString(),
                    $"Order #{payment.Order.OrderNumber}");

                if (!createIntentResult.Success)
                    return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, createIntentResult.Message);

                var paymentIntent = createIntentResult.Data;

                // Confirm the payment intent
                var confirmResult = await stripePaymentService.ConfirmPaymentIntentAsync(
                    paymentIntent.Id,
                    dto.StripePaymentMethodId);

                if (!confirmResult.Success)
                    return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, confirmResult.Message);

                // Update payment with Stripe transaction details
                payment.Method = PaymentMethod.Stripe;
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = paymentIntent.Id;
            }
            else
            {
                // Handle other payment methods (existing logic)
                payment.Method = dto.Method;
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = Guid.NewGuid().ToString("N");
            }

            if (payment.Order.Status == OrderStatus.Pending)
                payment.Order.Status = OrderStatus.Confirmed;

            await context.SaveChangesAsync();

            await notificationService.NotifyAsync(userId, "Пардохт анҷом ёфт",
                $"Пардохти фармоиши №{payment.Order.OrderNumber} ба маблағи {payment.Amount:0.00} сомонӣ бомуваффақият қабул шуд");

            return await orderService.GetOrderDetailAsync(userId, payment.OrderId);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion

    #region GetByOrderId

    public async Task<Response<PaymentDto>> GetByOrderIdAsync(int userId, int orderId)
    {
        try
        {
            var payment = await context.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Order.UserId == userId);

            return payment == null
                ? new Response<PaymentDto>(HttpStatusCode.NotFound,"Пардохт ёфт нашуд")
                : new Response<PaymentDto>(ToDto(payment));
        }
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion
    
    private static PaymentDto ToDto(Payment p) => new(p.Id, p.Amount, p.Method, p.Status, p.PaidAt);

}