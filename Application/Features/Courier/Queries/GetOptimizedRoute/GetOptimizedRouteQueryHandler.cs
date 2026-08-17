using System.Net;
using Application.Common.Interfaces;
using Application.Common.Utils;
using Application.Features.Courier.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetOptimizedRoute;

public class GetOptimizedRouteQueryHandler(
    IApplicationDbContext context,
    ILogger<GetOptimizedRouteQueryHandler> logger)
    : IRequestHandler<GetOptimizedRouteQuery, Response<OptimizedRouteDto>>
{
    public async Task<Response<OptimizedRouteDto>> Handle(GetOptimizedRouteQuery request, CancellationToken cancellationToken)
    {
        var courierUserId = request.CourierUserId;

        try
        {
            logger.LogInformation("Computing optimized route for courier user {CourierUserId}", courierUserId);

            var courier = await context.Couriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == courierUserId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {CourierUserId}", courierUserId);
                return new Response<OptimizedRouteDto>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }

            if (!courier.Latitude.HasValue || !courier.Longitude.HasValue)
            {
                logger.LogWarning("Courier {CourierId} has no current location", courier.Id);
                return new Response<OptimizedRouteDto>(HttpStatusCode.BadRequest,
                    "Ҷойгиршавии ҷории шумо номаълум аст — аввал GPS-ро фаъол кунед");
            }

            var activeOrders = await context.Orders
                .AsNoTracking()
                .Include(o => o.ShippingAddress)
                .Where(o =>
                    o.CourierId == courier.Id &&
                    o.Status != OrderStatus.Delivered &&
                    o.Status != OrderStatus.Cancelled &&
                    o.Status != OrderStatus.Returned)
                .ToListAsync(cancellationToken);

            if (activeOrders.Count == 0)
            {
                return new Response<OptimizedRouteDto>(
                    new OptimizedRouteDto(new List<RouteStopDto>(), 0, new List<string>()),
                    "Ҳеҷ фармоиши фаъол нест");
            }

            var withCoords = activeOrders
                .Where(o => o.ShippingAddress.Latitude.HasValue && o.ShippingAddress.Longitude.HasValue)
                .ToList();

            var withoutCoords = activeOrders
                .Where(o => !o.ShippingAddress.Latitude.HasValue || !o.ShippingAddress.Longitude.HasValue)
                .Select(o => o.OrderNumber)
                .ToList();

            if (withoutCoords.Count > 0)
                logger.LogWarning("{Count} orders skipped from route (no coordinates): {Orders}",
                    withoutCoords.Count, string.Join(", ", withoutCoords));

            var stops = new List<RouteStopDto>();
            var remaining = new List<Domain.Entities.OrderEntity.Order>(withCoords);

            var currentLat = courier.Latitude.Value;
            var currentLng = courier.Longitude.Value;
            var totalDistance = 0.0;
            var sequence = 1;

            while (remaining.Count > 0)
            {
                var nearest = remaining
                    .OrderBy(o => GeoHelper.DistanceKm(
                        currentLat, currentLng,
                        o.ShippingAddress.Latitude!.Value, o.ShippingAddress.Longitude!.Value))
                    .First();

                var distance = GeoHelper.DistanceKm(
                    currentLat, currentLng,
                    nearest.ShippingAddress.Latitude!.Value, nearest.ShippingAddress.Longitude!.Value);

                stops.Add(new RouteStopDto(
                    nearest.Id,
                    nearest.OrderNumber,
                    nearest.ShippingAddress.City,
                    nearest.ShippingAddress.Street,
                    nearest.ShippingAddress.Latitude,
                    nearest.ShippingAddress.Longitude,
                    sequence,
                    Math.Round(distance, 2)));

                totalDistance += distance;
                currentLat = nearest.ShippingAddress.Latitude!.Value;
                currentLng = nearest.ShippingAddress.Longitude!.Value;

                remaining.Remove(nearest);
                sequence++;
            }

            var result = new OptimizedRouteDto(stops, Math.Round(totalDistance, 2), withoutCoords);

            logger.LogInformation(
                "Route computed for courier {CourierId}: {StopCount} stops, {TotalDistance} km",
                courier.Id, stops.Count, result.TotalDistanceKm);

            return new Response<OptimizedRouteDto>(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error computing optimized route for courier user {CourierUserId}", courierUserId);
            return new Response<OptimizedRouteDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}