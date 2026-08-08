using Domain.Enums;

namespace Domain.DTOs.CourierDto;

public record CourierDto(
    int Id,
    int UserId,
    string FullName,
    string? PhoneNumber,
    CourierStatus Status,
    string VehicleType,
    double? Latitude,
    double? Longitude,
    DateTime? LastLocationUpdate,
    DateTime CreatedAt
);