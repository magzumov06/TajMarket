using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.ApproveReturnRequest;

public class ApproveReturnRequestCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    ILogger<ApproveReturnRequestCommandHandler> logger)
    : IRequestHandler<ApproveReturnRequestCommand, Response<ReturnRequestDto>>
{
    public async Task<Response<ReturnRequestDto>> Handle(ApproveReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var (returnRequestId, dto) = (request.ReturnRequestId, request.Dto);

        try
        {
            logger.LogInformation("Approving return request {ReturnRequestId}", returnRequestId);

            var returnRequest = await context.ReturnRequests
                .Include(rr => rr.Order)
                .FirstOrDefaultAsync(rr => rr.Id == returnRequestId, cancellationToken);

            if (returnRequest == null)
            {
                logger.LogWarning("Return request not found {ReturnRequestId}", returnRequestId);
                return new Response<ReturnRequestDto>(HttpStatusCode.NotFound, "Дархости баргардонӣ ёфт нашуд");
            }

            if (returnRequest.Status != ReturnStatus.Requested)
            {
                logger.LogWarning("Return request {ReturnRequestId} already resolved, status {Status}", returnRequestId, returnRequest.Status);
                return new Response<ReturnRequestDto>(HttpStatusCode.Conflict, "Ин дархост аллакай ҳал шудааст");
            }

            returnRequest.Status = ReturnStatus.Approved;
            returnRequest.AdminComment = dto.AdminComment;
            returnRequest.ResolvedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                returnRequest.UserId,
                "Дархости баргардонӣ тасдиқ шуд",
                $"Дархости баргардонии фармоиши №{returnRequest.Order.OrderNumber} тасдиқ шуд. Лутфан маҳсулотро баргардонед."), cancellationToken);

            logger.LogInformation("Return request {ReturnRequestId} approved", returnRequestId);

            return new Response<ReturnRequestDto>(ReturnRequestMapper.ToDto(returnRequest), "Дархост тасдиқ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error approving return request {ReturnRequestId}", returnRequestId);
            return new Response<ReturnRequestDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}