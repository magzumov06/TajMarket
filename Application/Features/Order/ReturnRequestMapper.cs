using Application.Features.Order.Dtos;

namespace Application.Features.Order;

internal static class ReturnRequestMapper
{
    public static ReturnRequestDto ToDto(Domain.Entities.OrderEntity.ReturnRequest rr) => new(
        rr.Id,
        rr.OrderId,
        rr.Order.OrderNumber,
        rr.Reason,
        rr.Status,
        rr.AdminComment,
        rr.RequestedAt,
        rr.ResolvedAt
    );
}