namespace Domain.DTOs.CourierDto;

public record CourierLocationDto(
    double? Latitude,
    double? Longitude,
    DateTime? LastLocationUpdate
);