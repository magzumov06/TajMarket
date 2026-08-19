namespace Application.Features.Courier.DTOs;

public record CreateCourierDto(
    int UserId,
    string VehicleType);