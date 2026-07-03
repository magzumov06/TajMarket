namespace Domain.DTOs.CouponDto;

public record CreateCouponDto(
    string Code,
    decimal DiscountPercent,
    decimal? MaxDiscountAmount,
    DateTime ExpiryDate,
    int? UsageLimit
);