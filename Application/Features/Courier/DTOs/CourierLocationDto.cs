namespace Application.Features.Courier.DTOs;

public record CourierLocationDto(
    double? Latitude,
    double? Longitude,
    DateTime? LastLocationUpdate
);