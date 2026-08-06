using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.CancelOrder;

public class CancelOrderCommand(int userId, int orderId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int OrderId { get; } = orderId;
}