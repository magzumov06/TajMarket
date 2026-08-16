using System.Net;
using Application.Common.Interfaces;
using Application.Features.Order.Dtos;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Queries.GetMyReturnRequests;

public class GetMyReturnRequestsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetMyReturnRequestsQueryHandler> logger)
    : IRequestHandler<GetMyReturnRequestsQuery, Response<List<ReturnRequestDto>>>
{
    public async Task<Response<List<ReturnRequestDto>>> Handle(GetMyReturnRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Retrieving return requests for user {UserId}", userId);

            var returnRequests = await context.ReturnRequests
                .AsNoTracking()
                .Include(rr => rr.Order)
                .Where(rr => rr.UserId == userId)
                .OrderByDescending(rr => rr.RequestedAt)
                .ToListAsync(cancellationToken);

            return new Response<List<ReturnRequestDto>>(returnRequests.Select(ReturnRequestMapper.ToDto).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving return requests for user {UserId}", userId);
            return new Response<List<ReturnRequestDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}