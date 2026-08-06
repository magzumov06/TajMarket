using System.Net;
using Application.Common.Interfaces;
using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Queries.GetOrderList;

public class GetOrderListQueryHandler(
    IApplicationDbContext context,
    ILogger<GetOrderListQueryHandler> logger)
    : IRequestHandler<GetOrderListQuery, PaginationResponse<List<OrderListDto>>>
{
    public async Task<PaginationResponse<List<OrderListDto>>> Handle(GetOrderListQuery request, CancellationToken cancellationToken)
    {
        var (userId, filter) = (request.UserId, request.Filter);

        try
        {
            logger.LogInformation("Retrieving order list for user {UserId} with filter {@Filter}", userId, filter);

            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
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
                "Retrieved {OrderCount} orders for user {UserId}, total {TotalCount}",
                orders.Count, userId, totalCount);

            return new PaginationResponse<List<OrderListDto>>(
                orders.Select(OrderMapper.ToListDto).ToList(),
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetOrderListAsync failed for user {UserId}", userId);
            return new PaginationResponse<List<OrderListDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}