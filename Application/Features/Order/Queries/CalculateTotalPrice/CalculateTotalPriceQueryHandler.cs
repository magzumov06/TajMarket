using System.Net;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Order.Queries.CalculateTotalPrice;

public class CalculateTotalPriceQueryHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IOptions<ShippingSetting> shippingSettings,
    ILogger<CalculateTotalPriceQueryHandler> logger)
    : IRequestHandler<CalculateTotalPriceQuery, Response<OrderPreviewDto>>
{
    public async Task<Response<OrderPreviewDto>> Handle(CalculateTotalPriceQuery request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation(
                "CalculateTotalPriceAsync started for user {UserId}, shippingAddress={ShippingAddressId}",
                userId, dto.ShippingAddressId);

            var (pricing, errorStatus, errorMessage) = await OrderPricingHelper.BuildPricingAsync(
                context, mediator, shippingSettings.Value, logger,
                userId, dto.ShippingAddressId, dto.CouponCode, cancellationToken);

            if (pricing == null)
                return new Response<OrderPreviewDto>(errorStatus, errorMessage!);

            var items = pricing.Cart.Items.Select(i => new OrderPreviewItemDto(
                i.ProductId,
                i.Product.Name,
                i.Product.Images.FirstOrDefault(im => im.IsMain)?.Url
                    ?? i.Product.Images.FirstOrDefault()?.Url,
                i.Quantity,
                OrderPricingHelper.UnitPrice(i),
                OrderPricingHelper.UnitPrice(i) * i.Quantity)).ToList();

            var preview = new OrderPreviewDto(
                items,
                pricing.SubTotal,
                pricing.DiscountAmount,
                pricing.ShippingCost,
                pricing.TotalAmount);

            logger.LogInformation(
                "CalculateTotalPriceAsync completed for user {UserId}, total {TotalAmount}",
                userId, pricing.TotalAmount);

            return new Response<OrderPreviewDto>(preview);
        }
        catch (Exception e)
        {
            logger.LogError(e, "CalculateTotalPriceAsync failed for user {UserId}", userId);
            return new Response<OrderPreviewDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}