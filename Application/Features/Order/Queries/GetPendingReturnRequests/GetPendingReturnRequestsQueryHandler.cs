using System.Net;
using Application.Common.Interfaces;
using Application.Features.Order.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Queries.GetPendingReturnRequests;

public class GetPendingReturnRequestsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetPendingReturnRequestsQueryHandler> logger)
    : IRequestHandler<GetPendingReturnRequestsQuery, Response<List<ReturnRequestDto>>>
{
    public async Task<Response<List<ReturnRequestDto>>> Handle(GetPendingReturnRequestsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving pending return requests");

            var returnRequests = await context.ReturnRequests
                .AsNoTracking()
                .Include(rr => rr.Order)
                .Where(rr => rr.Status == ReturnStatus.Requested || rr.Status == ReturnStatus.Approved)
                .OrderBy(rr => rr.RequestedAt)
                .ToListAsync(cancellationToken);

            return new Response<List<ReturnRequestDto>>(returnRequests.Select(ReturnRequestMapper.ToDto).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving pending return requests");
            return new Response<List<ReturnRequestDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}