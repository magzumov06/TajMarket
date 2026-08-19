namespace Application.Features.Courier.DTOs;

public record CourierMapDto(
    int Id,
    string FullName,
    double? Latitude,
    double? Longitude,
    string Status
);