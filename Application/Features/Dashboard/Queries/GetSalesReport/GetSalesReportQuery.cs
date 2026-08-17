using Application.Features.Dashboard.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;

namespace Application.Features.Dashboard.Queries.GetSalesReport;

public class GetSalesReportQuery(SalesReportPeriod period) : IRequest<Response<SalesReportDto>>
{
    public SalesReportPeriod Period { get; } = period;
}