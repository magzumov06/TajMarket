using System.Net;
using Application.Common.Interfaces;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Commands.ChangeCourierStatus;

public class ChangeCourierStatusCommandHandler(
    IApplicationDbContext context,
    IRealtimeNotifier realtimeNotifier,
    ILogger<ChangeCourierStatusCommandHandler> logger)
    : IRequestHandler<ChangeCourierStatusCommand, Response<string>>
{
    public async Task<Response<string>> Handle(ChangeCourierStatusCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("ChangeStatusAsync started for courier user {UserId} to status {Status}", userId, dto.Status);

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);
                return new Response<string>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }

            if (courier.Status == CourierStatus.Assigned && dto.Status != CourierStatus.Assigned)
            {
                var hasActiveOrder = await context.Orders.AnyAsync(o =>
                    o.CourierId == courier.Id &&
                    o.Status != OrderStatus.Delivered &&
                    o.Status != OrderStatus.Cancelled &&
                    o.Status != OrderStatus.Returned,
                    cancellationToken);

                if (hasActiveOrder)
                {
                    logger.LogWarning("Courier {CourierId} has an active order and cannot change status", courier.Id);
                    return new Response<string>(HttpStatusCode.BadRequest, "Шумо фармоиши фаъол доред, статусро иваз карда наметавонед");
                }
            }

            courier.Status = dto.Status;

            await context.SaveChangesAsync(cancellationToken);

            await realtimeNotifier.BroadcastCourierUpdateAsync(new CourierLiveUpdatePayload(
                courier.Id,
                courier.User.FullName,
                courier.Latitude,
                courier.Longitude,
                courier.Status.ToString()));

            logger.LogInformation("Courier {CourierId} status changed to {Status}", courier.Id, dto.Status);

            return new Response<string>(HttpStatusCode.OK, "Статус тағйир ёфт");
        }
        catch (Exception e)
        {
            logger.LogError(e, "ChangeStatusAsync failed for courier user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}