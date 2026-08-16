using System.Net;
using Application.Common.Interfaces;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Order.Dtos;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order.Commands.CompleteReturn;

public class CompleteReturnCommandHandler(
    IApplicationDbContext context,
    IMediator mediator,
    IStripePaymentService stripePaymentService,
    ILogger<CompleteReturnCommandHandler> logger)
    : IRequestHandler<CompleteReturnCommand, Response<ReturnRequestDto>>
{
    public async Task<Response<ReturnRequestDto>> Handle(CompleteReturnCommand request, CancellationToken cancellationToken)
    {
        var returnRequestId = request.ReturnRequestId;

        try
        {
            logger.LogInformation("Completing return request {ReturnRequestId}", returnRequestId);

            var returnRequest = await context.ReturnRequests
                .Include(rr => rr.Order)
                    .ThenInclude(o => o.OrderItems)
                .Include(rr => rr.Order)
                    .ThenInclude(o => o.Payment)
                .FirstOrDefaultAsync(rr => rr.Id == returnRequestId, cancellationToken);

            if (returnRequest == null)
            {
                logger.LogWarning("Return request not found {ReturnRequestId}", returnRequestId);
                return new Response<ReturnRequestDto>(HttpStatusCode.NotFound, "Дархости баргардонӣ ёфт нашуд");
            }

            if (returnRequest.Status != ReturnStatus.Approved)
            {
                logger.LogWarning(
                    "Return request {ReturnRequestId} cannot be completed, current status {Status}",
                    returnRequestId, returnRequest.Status);
                return new Response<ReturnRequestDto>(HttpStatusCode.BadRequest,
                    "Танҳо дархости тасдиқшуда пурра карда мешавад");
            }

            var order = returnRequest.Order;

            await using var transaction = await context.BeginTransactionAsync(cancellationToken);

            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var variant = await context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId.Value, cancellationToken);

                    if (variant != null)
                        variant.StockQuantity += item.Quantity;
                    else
                    {
                        var product = await context.Products
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                        if (product != null)
                            product.StockQuantity += item.Quantity;
                    }
                }
                else
                {
                    var product = await context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);

                    if (product != null)
                        product.StockQuantity += item.Quantity;
                }
            }

            if (order.Payment != null && order.Payment.Status == PaymentStatus.Completed)
            {
                if (order.Payment.Method == PaymentMethod.Stripe && !string.IsNullOrEmpty(order.Payment.TransactionId))
                {
                    var refundResult = await stripePaymentService.RefundPaymentAsync(order.Payment.TransactionId, order.Payment.Amount);

                    if (!refundResult.Success)
                    {
                        logger.LogError("Stripe refund failed for order {OrderId}: {Message}", order.Id, refundResult.Message);
                        return new Response<ReturnRequestDto>(HttpStatusCode.BadRequest,
                            $"Хатогии баргардонии пул: {refundResult.Message}");
                    }

                    logger.LogInformation("Stripe refund succeeded for order {OrderId}", order.Id);
                }

                order.Payment.Status = PaymentStatus.Refunded;
            }

            order.Status = OrderStatus.Returned;

            returnRequest.Status = ReturnStatus.Completed;
            returnRequest.ResolvedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await mediator.Send(new SendNotificationCommand(
                returnRequest.UserId,
                "Баргардонӣ анҷом ёфт",
                $"Баргардонии фармоиши №{order.OrderNumber} анҷом ёфт" +
                (order.Payment?.Method == PaymentMethod.Cash
                    ? ". Пули нақд ба шумо алоҳида баргардонида мешавад."
                    : ". Маблағ ба ҳисоби шумо баргардонида шуд.")), cancellationToken);

            logger.LogInformation("Return request {ReturnRequestId} completed for order {OrderId}", returnRequestId, order.Id);

            return new Response<ReturnRequestDto>(ReturnRequestMapper.ToDto(returnRequest), "Баргардонӣ анҷом ёфт");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error completing return request {ReturnRequestId}", returnRequestId);
            return new Response<ReturnRequestDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}