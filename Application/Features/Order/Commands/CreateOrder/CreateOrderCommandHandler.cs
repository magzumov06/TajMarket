using System.Net;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Features.Coupon.Commands.IncrementCouponUsage;
using Application.Features.Notification.Commands.SendNotification;
using Domain.Entities.OrderEntity;
using Domain.Entities.PaymentEntity;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Order.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IOptions<ShippingSetting> shippingSettings,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, Response<string>>
{
    public async Task<Response<string>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        logger.LogInformation(
            "CreateOrderAsync started: user={UserId}, shippingAddress={ShippingAddressId}, coupon={CouponCode}",
            userId, dto.ShippingAddressId, dto.CouponCode);

        try
        {
            var (pricing, errorStatus, errorMessage) = await OrderPricingHelper.BuildPricingAsync(
                context, mediator, shippingSettings.Value, logger,
                userId, dto.ShippingAddressId, dto.CouponCode, cancellationToken);

            if (pricing == null)
                return new Response<string>(errorStatus, errorMessage!);

            await using var transaction = await context.BeginTransactionAsync(cancellationToken);

            var order = new Domain.Entities.OrderEntity.Order
            {
                OrderNumber = OrderPricingHelper.GenerateOrderNumber(),
                DeliveryConfirmationCode = Guid.NewGuid().ToString("N"),  
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                SubTotal = pricing.SubTotal,
                ShippingCost = pricing.ShippingCost,
                DiscountAmount = pricing.DiscountAmount,
                TotalAmount = pricing.TotalAmount,
                Note = dto.Note,
                ShippingAddressId = dto.ShippingAddressId,

                OrderItems = pricing.Cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.Product.Name,
                    ProductImageUrl = i.Product.Images.FirstOrDefault(im => im.IsMain)?.Url
                        ?? i.Product.Images.FirstOrDefault()?.Url,
                    Quantity = i.Quantity,
                    UnitPrice = OrderPricingHelper.UnitPrice(i),
                    TotalPrice = OrderPricingHelper.UnitPrice(i) * i.Quantity
                }).ToList()
            };

            context.Orders.Add(order);

            foreach (var item in pricing.Cart.Items)
            {
                if (item.ProductVariant != null)
                    item.ProductVariant.StockQuantity -= item.Quantity;
                else
                    item.Product.StockQuantity -= item.Quantity;
            }

            context.Payments.Add(new Domain.Entities.PaymentEntity.Payment
            {
                Order = order,
                Amount = pricing.TotalAmount,
                Method = dto.PaymentMethod,
                Status = PaymentStatus.Pending
            });

            context.CartItems.RemoveRange(pricing.Cart.Items);

            await context.SaveChangesAsync(cancellationToken);

            if (pricing.AppliedCouponId.HasValue)
                await mediator.Send(new IncrementCouponUsageCommand(pricing.AppliedCouponId.Value), cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                userId,
                "Фармоиш сабт шуд",
                $"Фармоиши шумо №{order.OrderNumber} бо маблағи {pricing.TotalAmount:0.00} сомонӣ қабул шуд",
                alsoSms: true), cancellationToken);

            logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}", order.OrderNumber, userId);

            return new Response<string>(HttpStatusCode.OK, "Order successfully created");
        }
        catch (Exception e)
        {
            logger.LogError(e, "CreateOrderAsync failed for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}