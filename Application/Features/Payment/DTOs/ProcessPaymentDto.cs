using Domain.Enums;

namespace Application.Features.Payment.DTOs;

public record ProcessPaymentDto(
    int OrderId,
    PaymentMethod Method,
    string? CardToken,
    string? StripePaymentMethodId = null,
    string? StripeClientSecret = null
    );
