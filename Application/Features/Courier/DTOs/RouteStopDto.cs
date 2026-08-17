namespace Application.Features.Courier.Dtos;

public record RouteStopDto(
    int OrderId,
    string OrderNumber,
    string ShippingCity,
    string ShippingStreet,
    double? Latitude,
    double? Longitude,
    int SequenceNumber,
    double DistanceFromPreviousKm
);