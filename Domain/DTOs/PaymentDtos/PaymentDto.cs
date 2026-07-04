using Domain.Enums;

namespace Domain.DTOs.PaymentDtos;

public record PaymentDto(
    int Id,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime? PaidAt
    );
