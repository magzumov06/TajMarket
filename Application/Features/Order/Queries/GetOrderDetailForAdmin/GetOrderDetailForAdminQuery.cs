using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderDetailForAdmin;

public class GetOrderDetailForAdminQuery(int orderId, int requesterId, bool isAdmin) : IRequest<Response<OrderDetailDto>>
{
    public int OrderId { get; } = orderId;
    public int RequesterId { get; } = requesterId;
    public bool IsAdmin { get; } = isAdmin;
}