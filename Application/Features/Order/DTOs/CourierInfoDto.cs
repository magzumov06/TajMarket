using Domain.Enums;

namespace Application.Features.Order.DTOs;

public record CourierInfoDto(
    int Id,
    string FullName,
    string? PhoneNumber,
    CourierStatus Status
);