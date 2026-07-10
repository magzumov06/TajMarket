using System.Net;
using Domain.DTOs.CouponDto;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class CouponService(
    DataContext context,
    ILogger<CouponService> logger) : ICouponService
{
    public async Task<Response<List<CouponDto>>> GetAllActiveAsync()
    {
        try
        {
            logger.LogInformation("Retrieving all active coupons");

            var coupons = await context.Coupons
                .AsNoTracking()
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.UtcNow)
                .OrderByDescending(c => c.ExpiryDate)
                .ToListAsync();

            logger.LogInformation(
                "Retrieved {CouponCount} active coupons",
                coupons.Count);

            return new Response<List<CouponDto>>(
                coupons.Select(ToDto).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving active coupons");

            return new Response<List<CouponDto>>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }


    public async Task<Response<CouponDto>> CreateAsync(CreateCouponDto dto)
    {
        try
        {
            logger.LogInformation(
                "Creating coupon {CouponCode} with discount {DiscountPercent}",
                dto.Code,
                dto.DiscountPercent);


            if (dto.DiscountPercent is <= 0 or > 100)
            {
                logger.LogWarning(
                    "Invalid discount percent {DiscountPercent}",
                    dto.DiscountPercent);

                return new Response<CouponDto>(
                    HttpStatusCode.BadRequest,
                    "Фоизи тахфиф бояд аз 0 то 100 бошад");
            }


            if (dto.ExpiryDate <= DateTime.UtcNow)
            {
                logger.LogWarning(
                    "Invalid expiry date {ExpiryDate}",
                    dto.ExpiryDate);

                return new Response<CouponDto>(
                    HttpStatusCode.BadRequest,
                    "Санаи анҷом бояд дар оянда бошад");
            }


            var codeExists = await context.Coupons
                .AnyAsync(c => c.Code == dto.Code.Trim().ToUpper());


            if (codeExists)
            {
                logger.LogWarning(
                    "Coupon code already exists {CouponCode}",
                    dto.Code);

                return new Response<CouponDto>(
                    HttpStatusCode.Conflict,
                    "Ин коди купон аллакай истифода шудааст");
            }


            var coupon = new Coupon
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

            await context.SaveChangesAsync();


            logger.LogInformation(
                "Coupon created successfully {CouponId}",
                coupon.Id);


            return new Response<CouponDto>(ToDto(coupon));
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error creating coupon {CouponCode}",
                dto.Code);

            return new Response<CouponDto>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }


    public async Task<Response<string>> DeactivateAsync(string code)
    {
        try
        {
            logger.LogInformation(
                "Deactivating coupon {CouponCode}",
                code);


            if (string.IsNullOrWhiteSpace(code))
            {
                logger.LogWarning("Coupon code is empty");

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Coupon code is required");
            }


            var coupon = await context.Coupons
                .FirstOrDefaultAsync(
                    c => c.Code == code.Trim().ToUpper());


            if (coupon == null)
            {
                logger.LogWarning(
                    "Coupon not found {CouponCode}",
                    code);

                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "Купон ёфт нашуд");
            }


            coupon.IsActive = false;

            await context.SaveChangesAsync();


            logger.LogInformation(
                "Coupon deactivated successfully {CouponId}",
                coupon.Id);


            return new Response<string>(
                HttpStatusCode.OK,
                "Купон ғайрифаъол карда шуд");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error deactivating coupon {CouponCode}",
                code);

            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }


    public async Task<Response<CouponValidationResult>> ValidateAsync(
        string code,
        decimal orderAmount)
    {
        try
        {
            logger.LogInformation(
                "Validating coupon {CouponCode} for order amount {OrderAmount}",
                code,
                orderAmount);


            if (string.IsNullOrWhiteSpace(code))
            {
                logger.LogWarning("Coupon code is empty");

                return new Response<CouponValidationResult>(
                    HttpStatusCode.BadRequest,
                    "Coupon code is required");
            }


            var coupon = await context.Coupons
                .FirstOrDefaultAsync(
                    c => c.Code == code.Trim().ToUpper());


            if (coupon == null || !coupon.IsActive)
            {
                logger.LogWarning(
                    "Coupon not found or inactive {CouponCode}",
                    code);

                return new Response<CouponValidationResult>(
                    HttpStatusCode.NotFound,
                    "Купон ёфт нашуд ё фаъол нест");
            }


            if (coupon.ExpiryDate < DateTime.UtcNow)
            {
                logger.LogWarning(
                    "Coupon expired {CouponCode}",
                    code);

                return new Response<CouponValidationResult>(
                    HttpStatusCode.BadRequest,
                    "Мӯҳлати купон гузаштааст");
            }


            if (coupon.UsageLimit.HasValue &&
                coupon.UsedCount >= coupon.UsageLimit.Value)
            {
                logger.LogWarning(
                    "Coupon usage limit reached {CouponCode}",
                    code);

                return new Response<CouponValidationResult>(
                    HttpStatusCode.BadRequest,
                    "Лимити истифодаи ин купон тамом шудааст");
            }


            var discount = Math.Round(
                orderAmount * coupon.DiscountPercent / 100,
                2);


            if (coupon.MaxDiscountAmount.HasValue &&
                discount > coupon.MaxDiscountAmount.Value)
            {
                discount = coupon.MaxDiscountAmount.Value;
            }


            logger.LogInformation(
                "Coupon validated successfully {CouponId} discount {Discount}",
                coupon.Id,
                discount);


            return new Response<CouponValidationResult>(
                new CouponValidationResult(
                    coupon.Id,
                    discount));
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error validating coupon {CouponCode}",
                code);

            return new Response<CouponValidationResult>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }


    public async Task IncrementUsageAsync(int couponId)
    {
        try
        {
            logger.LogInformation(
                "Incrementing coupon usage {CouponId}",
                couponId);


            var coupon = await context.Coupons
                .FirstOrDefaultAsync(c => c.Id == couponId);


            if (coupon == null)
            {
                logger.LogWarning(
                    "Coupon not found {CouponId}",
                    couponId);

                return;
            }


            coupon.UsedCount++;

            await context.SaveChangesAsync();


            logger.LogInformation(
                "Coupon usage incremented successfully {CouponId}",
                couponId);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error incrementing coupon usage {CouponId}",
                couponId);
        }
    }


    private static CouponDto ToDto(Coupon c) => new(
        c.Code,
        c.DiscountPercent,
        c.MaxDiscountAmount,
        c.ExpiryDate);
}