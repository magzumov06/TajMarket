using System.Net;
using Application.Common.Interfaces;
using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Queries.GetOrderDetailForAdmin;

public class GetOrderDetailForAdminQueryHandler(
    IApplicationDbContext context,
    ILogger<GetOrderDetailForAdminQueryHandler> logger)
    : IRequestHandler<GetOrderDetailForAdminQuery, Response<OrderDetailDto>>
{
    public async Task<Response<OrderDetailDto>> Handle(GetOrderDetailForAdminQuery request, CancellationToken cancellationToken)
    {
        var (orderId, requesterId, isAdmin) = (request.OrderId, request.RequesterId, request.IsAdmin);

        try
        {
            logger.LogInformation("Retrieving order detail (admin/seller) {OrderId} by {RequesterId}", orderId, requesterId);

            var order = await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                    .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("Order not found {OrderId}", orderId);
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }

            if (!isAdmin)
            {
                var sellerProfile = await context.SellerProfiles
                    .FirstOrDefaultAsync(sp => sp.UserId == requesterId, cancellationToken);

                var ownsOrder = sellerProfile != null &&
                                order.OrderItems.Any(oi => oi.Product.SellerProfileId == sellerProfile.Id);

                if (!ownsOrder)
                {
                    logger.LogWarning("Seller {RequesterId} has no access to order {OrderId}", requesterId, orderId);
                    return new Response<OrderDetailDto>(HttpStatusCode.Forbidden, "Шумо ба ин фармоиш дастрасӣ надоред");
                }
            }

            logger.LogInformation("Order detail retrieved (admin/seller) {OrderId}", orderId);

            return new Response<OrderDetailDto>(OrderMapper.ToDetailDto(order));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving order detail (admin/seller) {OrderId}", orderId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}