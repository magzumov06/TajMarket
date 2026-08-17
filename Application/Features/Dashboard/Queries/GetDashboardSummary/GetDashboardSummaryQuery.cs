using Application.Features.Dashboard.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Dashboard.Queries.GetDashboardSummary;

public class GetDashboardSummaryQuery : IRequest<Response<DashboardSummaryDto>>;