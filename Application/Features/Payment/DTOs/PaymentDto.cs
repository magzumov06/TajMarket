using Domain.Enums;

namespace Application.Features.Payment.DTOs;

public record PaymentDto(
    int Id,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime? PaidAt
    );
