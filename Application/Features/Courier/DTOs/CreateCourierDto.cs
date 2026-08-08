namespace Domain.DTOs.CourierDto;

public record CreateCourierDto(
    int UserId,
    string VehicleType);