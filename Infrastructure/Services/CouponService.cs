using System.Net;
using Domain.DTOs.CouponDto;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class CouponService(DataContext context) : ICouponService
{
    public async Task<Response<List<CouponDto>>> GetAllActiveAsync()
    {
        try
        {
            Log.Information("Retrieving all active coupons");
            var coupons = await context.Coupons
                .AsNoTracking()
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.UtcNow)
                .OrderByDescending(c => c.ExpiryDate)
                .ToListAsync();

            Log.Information("Retrieved {CouponCount} active coupons", coupons.Count);
            return new Response<List<CouponDto>>(coupons.Select(ToDto).ToList());
        }
        catch (Exception e)
        {
            Log.Error(e, "Error retrieving active coupons");
            return new Response<List<CouponDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<CouponDto>> CreateAsync(CreateCouponDto dto)
    {
        try
        {
            Log.Information("Creating coupon {CouponCode} with discount {DiscountPercent}", dto.Code, dto.DiscountPercent);
            if (dto.DiscountPercent is <= 0 or > 100)
                return new Response<CouponDto>(HttpStatusCode.BadRequest,"Фоизи тахфиф бояд аз 0 то 100 бошад");

            if (dto.ExpiryDate <= DateTime.UtcNow)
                return new Response<CouponDto>(HttpStatusCode.BadRequest,"Санаи анҷом бояд дар оянда бошад");

            var codeExists = await context.Coupons.AnyAsync(c => c.Code == dto.Code);
            if (codeExists)
                return new Response<CouponDto>(HttpStatusCode.Conflict,"Ин коди купон аллакай истифода шудааст");

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

            Log.Information("Coupon created successfully: {CouponId}", coupon.Id);
            return new Response<CouponDto>(ToDto(coupon));
        }
        catch (Exception e)
        {
            Log.Error(e, "Error creating coupon {CouponCode}", dto.Code);
            return new Response<CouponDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> DeactivateAsync(string code)
    {
        try
        {
            Log.Information("Deactivating coupon {CouponCode}", code);
            if (string.IsNullOrWhiteSpace(code))
                return new Response<string>(HttpStatusCode.BadRequest, "Coupon code is required");

            var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpper());
            if (coupon == null)
                return new Response<string>(HttpStatusCode.NotFound, "Купон ёфт нашуд");

            coupon.IsActive = false;
            await context.SaveChangesAsync();
            
            Log.Information("Coupon deactivated successfully: {CouponId}", coupon.Id);
            return new Response<string>(HttpStatusCode.OK,"Купон ғайрифаъол карда шуд");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error deactivating coupon {CouponCode}", code);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<CouponValidationResult>> ValidateAsync(string code, decimal orderAmount)
    {
        try
        {
            Log.Information("Validating coupon {CouponCode} for order amount {OrderAmount}", code, orderAmount);
            if (string.IsNullOrWhiteSpace(code))
                return new Response<CouponValidationResult>(HttpStatusCode.BadRequest, "Coupon code is required");

            var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpper());

            if (coupon == null || !coupon.IsActive)
                return new Response<CouponValidationResult>(HttpStatusCode.NotFound,"Купон ёфт нашуд ё фаъол нест");

            if (coupon.ExpiryDate < DateTime.UtcNow)
                return new Response<CouponValidationResult>(HttpStatusCode.BadRequest,"Мӯҳлати купон гузаштааст");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return new Response<CouponValidationResult>(HttpStatusCode.BadRequest,"Лимити истифодаи ин купон тамом шудааст");

            var discount = Math.Round(orderAmount * coupon.DiscountPercent / 100, 2);
            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                discount = coupon.MaxDiscountAmount.Value;

            Log.Information("Coupon validated successfully: {CouponId} with discount {Discount}", coupon.Id, discount);
            return new Response<CouponValidationResult>(new CouponValidationResult(coupon.Id, discount));
        }
        catch (Exception e)
        {
            Log.Error(e, "Error validating coupon {CouponCode}", code);
            return new Response<CouponValidationResult>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task IncrementUsageAsync(int couponId)
    {
        var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId);
        if (coupon == null)
            return;

        coupon.UsedCount += 1;
        await context.SaveChangesAsync();    }
 
    private static CouponDto ToDto(Coupon c) => new(
        c.Code,
        c.DiscountPercent,
        c.MaxDiscountAmount, 
        c.ExpiryDate
        );

}