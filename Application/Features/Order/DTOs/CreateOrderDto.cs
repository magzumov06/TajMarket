using Domain.Enums;

namespace Application.Features.Order.DTOs;

public record CreateOrderDto(
    int ShippingAddressId,
    string? CouponCode,
    PaymentMethod PaymentMethod,
    string? Note
);