namespace Application.Features.Coupon.DTOs;

public record CreateCouponDto(
    string Code,
    decimal DiscountPercent,
    decimal? MaxDiscountAmount,
    DateTime ExpiryDate,
    int? UsageLimit
);