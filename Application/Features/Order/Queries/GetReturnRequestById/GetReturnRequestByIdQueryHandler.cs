using System.Net;
using Application.Common.Interfaces;
using Application.Features.Order.Dtos;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Queries.GetReturnRequestById;

public class GetReturnRequestByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetReturnRequestByIdQueryHandler> logger)
    : IRequestHandler<GetReturnRequestByIdQuery, Response<ReturnRequestDto>>
{
    public async Task<Response<ReturnRequestDto>> Handle(GetReturnRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var returnRequestId = request.ReturnRequestId;

        try
        {
            logger.LogInformation("Retrieving return request {ReturnRequestId}", returnRequestId);

            var returnRequest = await context.ReturnRequests
                .AsNoTracking()
                .Include(rr => rr.Order)
                .FirstOrDefaultAsync(rr => rr.Id == returnRequestId, cancellationToken);

            if (returnRequest == null)
            {
                logger.LogWarning("Return request not found {ReturnRequestId}", returnRequestId);
                return new Response<ReturnRequestDto>(HttpStatusCode.NotFound, "Дархости баргардонӣ ёфт нашуд");
            }

            return new Response<ReturnRequestDto>(ReturnRequestMapper.ToDto(returnRequest));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving return request {ReturnRequestId}", returnRequestId);
            return new Response<ReturnRequestDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}