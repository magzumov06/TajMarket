using Domain.Enums;

namespace Domain.DTOs.OrderDto;

public record CourierInfoDto(
    int Id,
    string FullName,
    string? PhoneNumber,
    CourierStatus Status
);