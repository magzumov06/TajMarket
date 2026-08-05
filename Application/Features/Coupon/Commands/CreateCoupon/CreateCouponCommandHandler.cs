using System.Net;
using Application.Common.Interfaces;
using Application.Features.Coupon.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Coupon.Commands.CreateCoupon;

public class CreateCouponCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateCouponCommandHandler> logger)
    : IRequestHandler<CreateCouponCommand, Response<CouponDto>>
{
    public async Task<Response<CouponDto>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        try
        {
            logger.LogInformation("Creating coupon {CouponCode} with discount {DiscountPercent}", dto.Code, dto.DiscountPercent);

            if (dto.DiscountPercent is <= 0 or > 100)
            {
                logger.LogWarning("Invalid discount percent {DiscountPercent}", dto.DiscountPercent);
                return new Response<CouponDto>(HttpStatusCode.BadRequest, "Фоизи тахфиф бояд аз 0 то 100 бошад");
            }

            if (dto.ExpiryDate <= DateTime.UtcNow)
            {
                logger.LogWarning("Invalid expiry date {ExpiryDate}", dto.ExpiryDate);
                return new Response<CouponDto>(HttpStatusCode.BadRequest, "Санаи анҷом бояд дар оянда бошад");
            }

            var codeExists = await context.Coupons
                .AnyAsync(c => c.Code == dto.Code.Trim().ToUpper(), cancellationToken);

            if (codeExists)
            {
                logger.LogWarning("Coupon code already exists {CouponCode}", dto.Code);
                return new Response<CouponDto>(HttpStatusCode.Conflict, "Ин коди купон аллакай истифода шудааст");
            }

            var coupon = new Domain.Entities.Coupon
            {
                Code = dto.Code.Trim().ToUpperInvariant(),
                DiscountPercent = dto.DiscountPercent,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                ExpiryDate = dto.ExpiryDate,
                UsageLimit = dto.UsageLimit,
                UsedCount = 0,
                IsActive = true
            };

            context.Coupons.Add(coupon);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Coupon created successfully {CouponId}", coupon.Id);

            return new Response<CouponDto>(CouponMapper.ToDto(coupon));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating coupon {CouponCode}", dto.Code);
            return new Response<CouponDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}