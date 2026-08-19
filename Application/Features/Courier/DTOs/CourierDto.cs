using Domain.Enums;

namespace Application.Features.Courier.DTOs;

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