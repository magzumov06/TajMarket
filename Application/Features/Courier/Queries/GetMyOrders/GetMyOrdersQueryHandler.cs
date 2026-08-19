using System.Net;
using Application.Common.Interfaces;
using Application.Features.Courier.DTOs;
using Domain.DTOs.CourierDto;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetMyOrders;

public class GetMyOrdersQueryHandler(
    IApplicationDbContext context,
    ILogger<GetMyOrdersQueryHandler> logger)
    : IRequestHandler<GetMyOrdersQuery, Response<List<CourierOrderDto>>>
{
    public async Task<Response<List<CourierOrderDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Retrieving active orders for courier user {UserId}", userId);

            var courier = await context.Couriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);
                return new Response<List<CourierOrderDto>>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }

            var orders = await context.Orders
                .AsNoTracking()
                .Include(o => o.Buyer)
                .Include(o => o.ShippingAddress)
                .Where(o =>
                    o.CourierId == courier.Id &&
                    o.Status != OrderStatus.Delivered &&
                    o.Status != OrderStatus.Cancelled &&
                    o.Status != OrderStatus.Returned)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {OrderCount} active orders for courier user {UserId}", orders.Count, userId);

            return new Response<List<CourierOrderDto>>(orders.Select(CourierMapper.ToOrderDto).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetMyOrdersAsync failed for user {UserId}", userId);
            return new Response<List<CourierOrderDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}