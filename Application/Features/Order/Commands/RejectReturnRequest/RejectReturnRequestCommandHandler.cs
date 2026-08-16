using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.RejectReturnRequest;

public class RejectReturnRequestCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    ILogger<RejectReturnRequestCommandHandler> logger)
    : IRequestHandler<RejectReturnRequestCommand, Response<ReturnRequestDto>>
{
    public async Task<Response<ReturnRequestDto>> Handle(RejectReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var (returnRequestId, dto) = (request.ReturnRequestId, request.Dto);

        try
        {
            logger.LogInformation("Rejecting return request {ReturnRequestId}", returnRequestId);

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

            if (string.IsNullOrWhiteSpace(dto.AdminComment))
            {
                logger.LogWarning("Rejection comment missing for return request {ReturnRequestId}", returnRequestId);
                return new Response<ReturnRequestDto>(HttpStatusCode.BadRequest, "Барои рад кардан, сабаб ҳатмист");
            }

            returnRequest.Status = ReturnStatus.Rejected;
            returnRequest.AdminComment = dto.AdminComment;
            returnRequest.ResolvedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                returnRequest.UserId,
                "Дархости баргардонӣ рад шуд",
                $"Дархости баргардонии фармоиши №{returnRequest.Order.OrderNumber} рад шуд. Сабаб: {dto.AdminComment}"), cancellationToken);

            logger.LogInformation("Return request {ReturnRequestId} rejected", returnRequestId);

            return new Response<ReturnRequestDto>(ReturnRequestMapper.ToDto(returnRequest), "Дархост рад шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error rejecting return request {ReturnRequestId}", returnRequestId);
            return new Response<ReturnRequestDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}