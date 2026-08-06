using System.Net;
using Application.Common.Interfaces;
using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Queries.GetOrderDetail;

public class GetOrderDetailQueryHandler(
    IApplicationDbContext context,
    ILogger<GetOrderDetailQueryHandler> logger)
    : IRequestHandler<GetOrderDetailQuery, Response<OrderDetailDto>>
{
    public async Task<Response<OrderDetailDto>> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var (orderId, userId) = (request.OrderId, request.UserId);

        try
        {
            logger.LogInformation("Retrieving order detail for order {OrderId} and user {UserId}", orderId, userId);

            var order = await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("Order not found {OrderId} for user {UserId}", orderId, userId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }

            logger.LogInformation("Order detail retrieved successfully {OrderId}", orderId);

            return new Response<OrderDetailDto>(OrderMapper.ToDetailDto(order));
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetOrderDetailAsync failed for order {OrderId}", orderId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}