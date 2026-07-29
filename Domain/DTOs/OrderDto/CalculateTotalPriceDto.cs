namespace Domain.DTOs.OrderDto;

public record CalculateTotalPriceDto(int ShippingAddressId, string? CouponCode);