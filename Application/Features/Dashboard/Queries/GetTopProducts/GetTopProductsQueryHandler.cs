using System.Net;
using Application.Common.Interfaces;
using Application.Features.Dashboard.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Dashboard.Queries.GetTopProducts;

public class GetTopProductsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetTopProductsQueryHandler> logger)
    : IRequestHandler<GetTopProductsQuery, Response<List<TopProductDto>>>
{
    private static readonly OrderStatus[] RelevantStatuses =
    {
        OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered
    };

    public async Task<Response<List<TopProductDto>>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving top {Count} products", request.Count);

            var topProducts = await context.OrderItems
                .AsNoTracking()
                .Where(oi => RelevantStatuses.Contains(oi.Order.Status))
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(request.Count)
                .ToListAsync(cancellationToken);

            var productIds = topProducts.Select(p => p.ProductId).ToList();

            var images = await context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, ImageUrl = p.Images.FirstOrDefault(i => i.IsMain)!.Url ?? p.Images.FirstOrDefault()!.Url })
                .ToDictionaryAsync(p => p.Id, p => p.ImageUrl, cancellationToken);

            var result = topProducts
                .Select(p => new TopProductDto(
                    p.ProductId,
                    p.ProductName,
                    images.TryGetValue(p.ProductId, out var url) ? url : null,
                    p.TotalSold,
                    p.TotalRevenue))
                .ToList();

            logger.LogInformation("Retrieved {Count} top products", result.Count);

            return new Response<List<TopProductDto>>(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving top products");
            return new Response<List<TopProductDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}