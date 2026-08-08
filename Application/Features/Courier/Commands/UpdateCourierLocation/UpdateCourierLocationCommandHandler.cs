using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Commands.UpdateCourierLocation;

public class UpdateCourierLocationCommandHandler(
    IApplicationDbContext context,
    IRealtimeNotifier realtimeNotifier,
    ILogger<UpdateCourierLocationCommandHandler> logger)
    : IRequestHandler<UpdateCourierLocationCommand, Response<string>>
{
    public async Task<Response<string>> Handle(UpdateCourierLocationCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("UpdateLocationAsync started for courier user {UserId}", userId);

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);
                return new Response<string>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }

            courier.Latitude = dto.Latitude;
            courier.Longitude = dto.Longitude;
            courier.LastLocationUpdate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            await realtimeNotifier.BroadcastCourierUpdateAsync(new CourierLiveUpdatePayload(
                courier.Id,
                courier.User.FullName,
                courier.Latitude,
                courier.Longitude,
                courier.Status.ToString()));

            logger.LogInformation("Location updated successfully for courier {CourierId}", courier.Id);

            return new Response<string>(HttpStatusCode.OK, "Ҷойгиршавӣ навсозӣ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateLocationAsync failed for courier user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}