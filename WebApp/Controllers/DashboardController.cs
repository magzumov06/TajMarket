using Application.Features.Dashboard.Queries.GetDashboardSummary;
using Application.Features.Dashboard.Queries.GetSalesReport;
using Application.Features.Dashboard.Queries.GetTopProducts;
using Application.Features.Dashboard.Queries.GetTopSellers;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController(IMediator mediator) : BaseApiController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var res = await mediator.Send(new GetDashboardSummaryQuery());
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("sales-report")]
    public async Task<IActionResult> GetSalesReport([FromQuery] SalesReportPeriod period = SalesReportPeriod.Last7Days)
    {
        var res = await mediator.Send(new GetSalesReportQuery(period));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 10)
    {
        var res = await mediator.Send(new GetTopProductsQuery(count));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("top-sellers")]
    public async Task<IActionResult> GetTopSellers([FromQuery] int count = 10)
    {
        var res = await mediator.Send(new GetTopSellersQuery(count));
        return StatusCode((int)res.StatusCode, res);
    }
}