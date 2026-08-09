using System.Net;
using Application.Common.Interfaces;
using Application.Common.Utils;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Commands.AutoAssignCourierToOrder;

public class AutoAssignCourierToOrderCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IRealtimeNotifier realtimeNotifier,
    ILogger<AutoAssignCourierToOrderCommandHandler> logger)
    : IRequestHandler<AutoAssignCourierToOrderCommand, Response<string>>
{
    public async Task<Response<string>> Handle(AutoAssignCourierToOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = request.OrderId;

        try
        {
            logger.LogInformation("AutoAssignCourierToOrderAsync started for order {OrderId}", orderId);

            var (order, error) = await CourierAssignmentHelper.ValidateOrderForAssignmentAsync(
                context, orderId, logger, includeAddress: true, cancellationToken);

            if (error != null)
                return error;

            var availableCouriers = await context.Couriers
                .Include(c => c.User)
                .Where(c => c.Status == CourierStatus.Available)
                .ToListAsync(cancellationToken);

            if (availableCouriers.Count == 0)
            {
                logger.LogWarning("No available couriers for order {OrderId}", orderId);
                return new Response<string>(HttpStatusCode.BadRequest, "Дар айни замон курьери дастрас нест");
            }

            Domain.Entities.UserEntity.Courier nearest;

            var destLat = order!.ShippingAddress?.Latitude;
            var destLng = order.ShippingAddress?.Longitude;

            var couriersWithLocation = availableCouriers
                .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
                .ToList();

            if (destLat.HasValue && destLng.HasValue && couriersWithLocation.Count > 0)
            {
                nearest = couriersWithLocation
                    .OrderBy(c => GeoHelper.DistanceKm(
                        destLat.Value, destLng.Value,
                        c.Latitude!.Value, c.Longitude!.Value))
                    .First();

                logger.LogInformation("Nearest courier {CourierId} selected for order {OrderId} by distance", nearest.Id, orderId);
            }
            else
            {
                nearest = availableCouriers
                    .OrderByDescending(c => c.LastLocationUpdate ?? DateTime.MinValue)
                    .First();

                logger.LogInformation("Fallback courier {CourierId} selected for order {OrderId} (no coordinates available)", nearest.Id, orderId);
            }

            await CourierAssignmentHelper.PerformAssignmentAsync(context, mediator, realtimeNotifier, order, nearest, cancellationToken);

            logger.LogInformation("Courier {CourierId} auto-assigned to order {OrderId}", nearest.Id, orderId);

            return new Response<string>(HttpStatusCode.OK, "Наздиктарин курьер бо муваффақият таъин шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "AutoAssignCourierToOrderAsync failed for order {OrderId}", orderId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}