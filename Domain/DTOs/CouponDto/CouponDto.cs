namespace Domain.DTOs.CouponDto;

public record CouponDto(string Code, decimal DiscountPercent, decimal? MaxDiscountAmount, DateTime ExpiryDate);
