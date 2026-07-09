using Domain.Enums;

namespace Domain.DTOs.PaymentDtos;

public record ProcessPaymentDto(
    int OrderId,
    PaymentMethod Method,
    string? CardToken,
    string? StripePaymentMethodId = null,
    string? StripeClientSecret = null
    );
