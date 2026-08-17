using System.Net;
using Application.Common.Interfaces;
using Application.Features.Dashboard.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Dashboard.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler(
    IApplicationDbContext context,
    ILogger<GetDashboardSummaryQueryHandler> logger)
    : IRequestHandler<GetDashboardSummaryQuery, Response<DashboardSummaryDto>>
{
    public async Task<Response<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving dashboard summary");

            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var deliveredOrRelevantStatuses = new[]
            {
                OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered
            };

            var todayRevenue = await context.Orders
                .Where(o => o.OrderDate >= today && deliveredOrRelevantStatuses.Contains(o.Status))
                .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0;

            var todayOrdersCount = await context.Orders
                .CountAsync(o => o.OrderDate >= today, cancellationToken);

            var monthRevenue = await context.Orders
                .Where(o => o.OrderDate >= monthStart && deliveredOrRelevantStatuses.Contains(o.Status))
                .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0;

            var monthOrdersCount = await context.Orders
                .CountAsync(o => o.OrderDate >= monthStart, cancellationToken);

            var pendingOrdersCount = await context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

            var activeCouriersCount = await context.Couriers
                .CountAsync(c => c.Status == CourierStatus.Available || c.Status == CourierStatus.Assigned, cancellationToken);

            var pendingReturnRequestsCount = await context.ReturnRequests
                .CountAsync(rr => rr.Status == ReturnStatus.Requested, cancellationToken);

            var totalActiveProducts = await context.Products
                .CountAsync(p => p.IsActive, cancellationToken);

            var totalActiveSellers = await context.SellerProfiles
                .CountAsync(sp => sp.IsVerified, cancellationToken);

            var totalCustomers = await context.Users
                .CountAsync(u => u.IsActive, cancellationToken);

            var summary = new DashboardSummaryDto(
                todayRevenue, todayOrdersCount,
                monthRevenue, monthOrdersCount,
                pendingOrdersCount, activeCouriersCount,
                pendingReturnRequestsCount, totalActiveProducts,
                totalActiveSellers, totalCustomers);

            logger.LogInformation("Dashboard summary retrieved successfully");

            return new Response<DashboardSummaryDto>(summary);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving dashboard summary");
            return new Response<DashboardSummaryDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}