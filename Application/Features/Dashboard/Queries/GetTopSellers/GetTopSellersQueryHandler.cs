using System.Net;
using Application.Common.Interfaces;
using Application.Features.Dashboard.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Dashboard.Queries.GetTopSellers;

public class GetTopSellersQueryHandler(
    IApplicationDbContext context,
    ILogger<GetTopSellersQueryHandler> logger)
    : IRequestHandler<GetTopSellersQuery, Response<List<TopSellerDto>>>
{
    private static readonly OrderStatus[] RelevantStatuses =
    {
        OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered
    };

    public async Task<Response<List<TopSellerDto>>> Handle(GetTopSellersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving top {Count} sellers", request.Count);

            var topSellers = await context.OrderItems
                .AsNoTracking()
                .Where(oi => RelevantStatuses.Contains(oi.Order.Status))
                .GroupBy(oi => new { oi.Product.SellerProfileId, oi.Product.SellerProfile.StoreName })
                .Select(g => new TopSellerDto(
                    g.Key.SellerProfileId,
                    g.Key.StoreName,
                    g.Select(oi => oi.OrderId).Distinct().Count(),
                    g.Sum(oi => oi.TotalPrice)))
                .OrderByDescending(x => x.TotalRevenue)
                .Take(request.Count)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} top sellers", topSellers.Count);

            return new Response<List<TopSellerDto>>(topSellers);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving top sellers");
            return new Response<List<TopSellerDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}