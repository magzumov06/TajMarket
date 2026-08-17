using System.Net;
using Application.Common.Interfaces;
using Application.Features.Dashboard.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Dashboard.Queries.GetSalesReport;

public class GetSalesReportQueryHandler(
    IApplicationDbContext context,
    ILogger<GetSalesReportQueryHandler> logger)
    : IRequestHandler<GetSalesReportQuery, Response<SalesReportDto>>
{
    private static readonly OrderStatus[] RevenueStatuses =
    {
        OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered
    };

    public async Task<Response<SalesReportDto>> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving sales report for period {Period}", request.Period);

            var (startDate, groupByMonth) = request.Period switch
            {
                SalesReportPeriod.Last7Days => (DateTime.UtcNow.Date.AddDays(-6), false),
                SalesReportPeriod.Last30Days => (DateTime.UtcNow.Date.AddDays(-29), false),
                SalesReportPeriod.Last12Months => (DateTime.UtcNow.Date.AddMonths(-11), true),
                _ => (DateTime.UtcNow.Date.AddDays(-6), false)
            };

            var orders = await context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= startDate && RevenueStatuses.Contains(o.Status))
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync(cancellationToken);

            List<SalesReportPointDto> points;

            if (groupByMonth)
            {
                points = orders
                    .GroupBy(o => new DateTime(o.OrderDate.Year, o.OrderDate.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new SalesReportPointDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
                    .ToList();
            }
            else
            {
                points = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new SalesReportPointDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
                    .ToList();
            }

            var report = new SalesReportDto(points, orders.Sum(o => o.TotalAmount), orders.Count);

            logger.LogInformation("Sales report retrieved: {PointCount} points, {TotalOrders} orders", points.Count, orders.Count);

            return new Response<SalesReportDto>(report);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving sales report");
            return new Response<SalesReportDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}