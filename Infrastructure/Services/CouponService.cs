using System.Net;
using Domain.DTOs.CouponDto;
using Domain.Entities;
using Domain.Resposes;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CouponService(DataContext context) : ICouponService
{
    public async Task<Response<List<CouponDto>>> GetAllActiveAsync()
    {
        try
        {
            var coupons = await context.Coupons
                .AsNoTracking()
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.UtcNow)
                .OrderByDescending(c => c.ExpiryDate)
                .ToListAsync();

            return new Response<List<CouponDto>>(coupons.Select(ToDto).ToList());
        }
        catch (Exception e)
        {
            return new Response<List<CouponDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<CouponDto>> CreateAsync(CreateCouponDto dto)
    {
        try
        {
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

            return new Response<CouponDto>(ToDto(coupon));
        }
        catch (Exception e)
        {
            return new Response<CouponDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> DeactivateAsync(string code)
    {
        try
        {
            var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
            if (coupon == null)
                return new Response<string>("Купон ёфт нашуд");

            coupon.IsActive = false;
            await context.SaveChangesAsync();
            
            return new Response<string>( HttpStatusCode.OK,"Купон ғайрифаъол карда шуд");        }
        
        catch (Exception e)
        {
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<CouponValidationResult>> ValidateAsync(string code, decimal orderAmount)
    {
        try
        {
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

            return new Response<CouponValidationResult>(new CouponValidationResult(coupon.Id, discount));
        }
        catch (Exception e)
        {
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