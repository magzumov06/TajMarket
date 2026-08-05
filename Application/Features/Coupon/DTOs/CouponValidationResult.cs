namespace Application.Features.Coupon.Dtos;

public record CouponValidationResult(
    int CouponId, 
    decimal DiscountAmount);