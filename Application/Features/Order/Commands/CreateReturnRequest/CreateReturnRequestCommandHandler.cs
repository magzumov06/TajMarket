// Application/Features/Order/Commands/CreateReturnRequest/CreateReturnRequestCommandHandler.cs
using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.Dtos;
using Domain.Entities.OrderEntity;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.CreateReturnRequest;

public class CreateReturnRequestCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    ILogger<CreateReturnRequestCommandHandler> logger)
    : IRequestHandler<CreateReturnRequestCommand, Response<ReturnRequestDto>>
{
    private const int ReturnWindowDays = 14;

    public async Task<Response<ReturnRequestDto>> Handle(CreateReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("Creating return request for order {OrderId} by user {UserId}", dto.OrderId, userId);

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return new Response<ReturnRequestDto>(HttpStatusCode.BadRequest, "Сабаби баргардонӣ ҳатмист");

            var order = await context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("Order not found {OrderId} for user {UserId}", dto.OrderId, userId);
                return new Response<ReturnRequestDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }

            if (order.Status != OrderStatus.Delivered)
            {
                logger.LogWarning("Order {OrderId} is not delivered, cannot request return", dto.OrderId);
                return new Response<ReturnRequestDto>(HttpStatusCode.BadRequest,
                    "Танҳо фармоиши расонидашударо баргардонида метавонед");
            }

            if (!order.CustomerConfirmedAt.HasValue)
            {
                logger.LogWarning("Order {OrderId} delivery not confirmed by customer yet", dto.OrderId);
                return new Response<ReturnRequestDto>(HttpStatusCode.BadRequest,
                    "Аввал қабули фармоишро тасдиқ кунед, баъд дархости баргардонӣ фиристед");
            }

            var daysSinceDelivery = (DateTime.UtcNow - order.CustomerConfirmedAt.Value).TotalDays;

            if (daysSinceDelivery > ReturnWindowDays)
            {
                logger.LogWarning("Return window expired for order {OrderId}", dto.OrderId);
                return new Response<ReturnRequestDto>(HttpStatusCode.BadRequest,
                    $"Мӯҳлати баргардонӣ ({ReturnWindowDays} рӯз) гузаштааст");
            }

            var alreadyExists = await context.ReturnRequests
                .AnyAsync(rr => rr.OrderId == dto.OrderId &&
                                 rr.Status != ReturnStatus.Rejected,
                    cancellationToken);

            if (alreadyExists)
            {
                logger.LogWarning("Return request already exists for order {OrderId}", dto.OrderId);
                return new Response<ReturnRequestDto>(HttpStatusCode.Conflict,
                    "Барои ин фармоиш аллакай дархости баргардонӣ ҳаст");
            }

            var returnRequest = new ReturnRequest
            {
                OrderId = order.Id,
                UserId = userId,
                Reason = dto.Reason,
                Status = ReturnStatus.Requested
            };

            context.ReturnRequests.Add(returnRequest);
            await context.SaveChangesAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                userId,
                "Дархости баргардонӣ қабул шуд",
                $"Дархости баргардонии фармоиши №{order.OrderNumber} ба баррасӣ фиристода шуд"), cancellationToken);

            logger.LogInformation("Return request {ReturnRequestId} created for order {OrderId}", returnRequest.Id, order.Id);

            var created = await context.ReturnRequests
                .Include(rr => rr.Order)
                .FirstAsync(rr => rr.Id == returnRequest.Id, cancellationToken);

            return new Response<ReturnRequestDto>(ReturnRequestMapper.ToDto(created), "Дархости баргардонӣ фиристода шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating return request for order {OrderId}", dto.OrderId);
            return new Response<ReturnRequestDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}