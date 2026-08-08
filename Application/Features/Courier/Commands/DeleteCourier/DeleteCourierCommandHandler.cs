using System.Net;
using Application.Common.Interfaces;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Commands.DeleteCourier;

public class DeleteCourierCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    ILogger<DeleteCourierCommandHandler> logger)
    : IRequestHandler<DeleteCourierCommand, Response<string>>
{
    private const string CourierRole = "Courier";

    public async Task<Response<string>> Handle(DeleteCourierCommand request, CancellationToken cancellationToken)
    {
        var courierId = request.CourierId;

        try
        {
            logger.LogInformation("DeleteCourierAsync started for courier {CourierId}", courierId);

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);
                return new Response<string>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }

            var hasActiveOrders = await context.Orders.AnyAsync(o =>
                o.CourierId == courierId &&
                o.Status != OrderStatus.Delivered &&
                o.Status != OrderStatus.Cancelled &&
                o.Status != OrderStatus.Returned,
                cancellationToken);

            if (hasActiveOrders)
            {
                logger.LogWarning("Courier {CourierId} has active orders and cannot be deleted", courierId);
                return new Response<string>(HttpStatusCode.Conflict, "Курьер фармоишҳои фаъол дорад, аввал онҳоро анҷом диҳед");
            }

            context.Couriers.Remove(courier);
            await context.SaveChangesAsync(cancellationToken);

            if (courier.User != null && await identityService.IsInRoleAsync(courier.UserId, CourierRole, cancellationToken))
            {
                await identityService.RemoveFromRoleAsync(courier.UserId, CourierRole, cancellationToken);
            }

            logger.LogInformation("Courier {CourierId} deleted successfully", courierId);

            return new Response<string>(HttpStatusCode.OK, "Курьер бо муваффақият нест карда шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "DeleteCourierAsync failed for courier {CourierId}", courierId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}