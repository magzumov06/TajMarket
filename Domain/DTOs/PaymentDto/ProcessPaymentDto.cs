using Domain.Enums;

namespace Domain.DTOs.PaymentDto;

public record ProcessPaymentDto(int OrderId, PaymentMethod Method, string? CardToken);
