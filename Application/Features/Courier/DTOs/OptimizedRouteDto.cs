namespace Application.Features.Courier.Dtos;

public record OptimizedRouteDto(
    List<RouteStopDto> Stops,
    double TotalDistanceKm,
    List<string> SkippedOrderNumbers  
);