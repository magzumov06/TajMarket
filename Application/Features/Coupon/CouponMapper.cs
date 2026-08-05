using Application.Features.Coupon.DTOs;

namespace Application.Features.Coupon;

internal static class CouponMapper
{
    public static CouponDto ToDto(Domain.Entities.Coupon c) => new(
        c.Code,
        c.DiscountPercent,
        c.MaxDiscountAmount,
        c.ExpiryDate);
}