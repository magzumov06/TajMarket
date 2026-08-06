using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderDetail;

public class GetOrderDetailQuery(int orderId, int userId) : IRequest<Response<OrderDetailDto>>
{
    public int OrderId { get; } = orderId;
    public int UserId { get; } = userId;
}