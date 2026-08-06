using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.CompleteOrder;

public class CompleteOrderCommand(int userId, int orderId) : IRequest<Response<OrderDetailDto>>
{
    public int UserId { get; } = userId;
    public int OrderId { get; } = orderId;
}