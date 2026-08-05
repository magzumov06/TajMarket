namespace Application.Features.Order.DTOs;

public record CalculateTotalPriceDto(
    int ShippingAddressId,
    string? CouponCode);