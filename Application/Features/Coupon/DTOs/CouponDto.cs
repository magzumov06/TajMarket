namespace Application.Features.Coupon.DTOs;

public record CouponDto(
    string Code,
    decimal DiscountPercent, 
    decimal? MaxDiscountAmount,
    DateTime ExpiryDate);
