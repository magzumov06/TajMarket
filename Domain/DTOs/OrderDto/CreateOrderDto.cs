using Domain.Enums;

namespace Domain.DTOs.OrderDto;

public record CreateOrderDto(
    int ShippingAddressId,
    string? CouponCode,
    PaymentMethod PaymentMethod,
    string? Note
);