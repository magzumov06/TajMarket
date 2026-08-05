using System.Net;
using Application.Common.Interfaces;
using Application.Features.Coupon.Dtos;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Coupon.Queries.ValidateCoupon;

public class ValidateCouponQueryHandler(
    IApplicationDbContext context,
    ILogger<ValidateCouponQueryHandler> logger)
    : IRequestHandler<ValidateCouponQuery, Response<CouponValidationResult>>
{
    public async Task<Response<CouponValidationResult>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var (code, orderAmount) = (request.Code, request.OrderAmount);

        try
        {
            logger.LogInformation("Validating coupon {CouponCode} for order amount {OrderAmount}", code, orderAmount);

            if (string.IsNullOrWhiteSpace(code))
            {
                logger.LogWarning("Coupon code is empty");
                return new Response<CouponValidationResult>(HttpStatusCode.BadRequest, "Coupon code is required");
            }

            var coupon = await context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpper(), cancellationToken);

            if (coupon == null || !coupon.IsActive)
            {
                logger.LogWarning("Coupon not found or inactive {CouponCode}", code);
                return new Response<CouponValidationResult>(HttpStatusCode.NotFound, "Купон ёфт нашуд ё фаъол нест");
            }

            if (coupon.ExpiryDate < DateTime.UtcNow)
            {
                logger.LogWarning("Coupon expired {CouponCode}", code);
                return new Response<CouponValidationResult>(HttpStatusCode.BadRequest, "Мӯҳлати купон гузаштааст");
            }

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            {
                logger.LogWarning("Coupon usage limit reached {CouponCode}", code);
                return new Response<CouponValidationResult>(HttpStatusCode.BadRequest, "Лимити истифодаи ин купон тамом шудааст");
            }

            var discount = Math.Round(orderAmount * coupon.DiscountPercent / 100, 2);

            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                discount = coupon.MaxDiscountAmount.Value;

            logger.LogInformation("Coupon validated successfully {CouponId} discount {Discount}", coupon.Id, discount);

            return new Response<CouponValidationResult>(new CouponValidationResult(coupon.Id, discount));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error validating coupon {CouponCode}", code);
            return new Response<CouponValidationResult>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}