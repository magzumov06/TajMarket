namespace Domain.DTOs.CourierDto;

public record CourierMapDto(
    int Id,
    string FullName,
    double? Latitude,
    double? Longitude,
    string Status
);