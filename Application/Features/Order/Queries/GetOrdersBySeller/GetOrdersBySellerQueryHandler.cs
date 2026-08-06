using System.Net;
using Application.Common.Interfaces;
using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Queries.GetOrdersBySeller;

public class GetOrdersBySellerQueryHandler(
    IApplicationDbContext context,
    ILogger<GetOrdersBySellerQueryHandler> logger)
    : IRequestHandler<GetOrdersBySellerQuery, PaginationResponse<List<OrderListDto>>>
{
    public async Task<PaginationResponse<List<OrderListDto>>> Handle(GetOrdersBySellerQuery request, CancellationToken cancellationToken)
    {
        var (sellerUserId, filter) = (request.SellerUserId, request.Filter);

        try
        {
            logger.LogInformation(
                "Retrieving orders for seller user {SellerUserId} with filter {@Filter}", sellerUserId, filter);

            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == sellerUserId, cancellationToken);

            if (sellerProfile == null)
            {
                logger.LogWarning("Seller profile not found for user {SellerUserId}", sellerUserId);
                return new PaginationResponse<List<OrderListDto>>(HttpStatusCode.NotFound, "Seller profile not found");
            }

            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => oi.Product.SellerProfileId == sellerProfile.Id))
                .AsQueryable();

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            query = filter.SortBy switch
            {
                "date_asc" => query.OrderBy(o => o.OrderDate),
                _ => query.OrderByDescending(o => o.OrderDate)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            logger.LogInformation(
                "Retrieved {OrderCount} orders for seller user {SellerUserId}, total {TotalCount}",
                orders.Count, sellerUserId, totalCount);

            return new PaginationResponse<List<OrderListDto>>(
                orders.Select(OrderMapper.ToListDto).ToList(),
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetBySellerIdAsync failed for seller user {SellerUserId}", sellerUserId);
            return new PaginationResponse<List<OrderListDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}