using System.Net;
using Application.Common.Interfaces;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Commands.AssignCourierToOrder;

public class AssignCourierToOrderCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IRealtimeNotifier realtimeNotifier,
    ILogger<AssignCourierToOrderCommandHandler> logger)
    : IRequestHandler<AssignCourierToOrderCommand, Response<string>>
{
    public async Task<Response<string>> Handle(AssignCourierToOrderCommand request, CancellationToken cancellationToken)
    {
        var (orderId, courierId) = (request.OrderId, request.CourierId);

        try
        {
            logger.LogInformation("AssignCourierToOrderAsync started for order {OrderId} and courier {CourierId}", orderId, courierId);

            var (order, error) = await CourierAssignmentHelper.ValidateOrderForAssignmentAsync(
                context, orderId, logger, includeAddress: false, cancellationToken);

            if (error != null)
                return error;

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);
                return new Response<string>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }

            if (courier.Status != CourierStatus.Available)
            {
                logger.LogWarning("Courier {CourierId} is not available, current status {Status}", courierId, courier.Status);
                return new Response<string>(HttpStatusCode.BadRequest, "Курьер дар айни замон дастрас нест");
            }

            await CourierAssignmentHelper.PerformAssignmentAsync(context, mediator, realtimeNotifier, order!, courier, cancellationToken);

            logger.LogInformation("Courier {CourierId} assigned to order {OrderId}", courierId, orderId);

            return new Response<string>(HttpStatusCode.OK, "Курьер бо муваффақият ба фармоиш таъин шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "AssignCourierToOrderAsync failed for order {OrderId} and courier {CourierId}", orderId, courierId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}