namespace Domain.DTOs.CourierDto;

public record CourierLiveUpdateDto(
    int Id,
    string FullName,
    double? Latitude,
    double? Longitude,
    string Status
);