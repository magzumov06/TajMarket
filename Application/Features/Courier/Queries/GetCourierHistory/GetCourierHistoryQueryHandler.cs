using System.Net;
using Application.Common.Interfaces;
using Application.Features.Courier.DTOs;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetCourierHistory;

public class GetCourierHistoryQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCourierHistoryQueryHandler> logger)
    : IRequestHandler<GetCourierHistoryQuery, PaginationResponse<List<CourierOrderDto>>>
{
    public async Task<PaginationResponse<List<CourierOrderDto>>> Handle(GetCourierHistoryQuery request, CancellationToken cancellationToken)
    {
        var (userId, filter) = (request.UserId, request.Filter);

        try
        {
            logger.LogInformation("Retrieving order history for courier user {UserId} with filter {@Filter}", userId, filter);

            var courier = await context.Couriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);
                return new PaginationResponse<List<CourierOrderDto>>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }

            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.Buyer)
                .Include(o => o.ShippingAddress)
                .Where(o =>
                    o.CourierId == courier.Id &&
                    (o.Status == OrderStatus.Delivered ||
                     o.Status == OrderStatus.Cancelled ||
                     o.Status == OrderStatus.Returned))
                .AsQueryable();

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

            var items = orders.Select(CourierMapper.ToOrderDto).ToList();

            logger.LogInformation(
                "Retrieved {ItemCount} history orders for courier user {UserId}, total {TotalCount}",
                items.Count, userId, totalCount);

            return new PaginationResponse<List<CourierOrderDto>>(items, totalCount, filter.PageNumber, filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetHistoryAsync failed for user {UserId}", userId);
            return new PaginationResponse<List<CourierOrderDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}