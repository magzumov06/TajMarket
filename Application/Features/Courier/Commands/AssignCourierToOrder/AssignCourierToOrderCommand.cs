using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Commands.AssignCourierToOrder;

public class AssignCourierToOrderCommand(int orderId, int courierId) : IRequest<Response<string>>
{
    public int OrderId { get; } = orderId;
    public int CourierId { get; } = courierId;
}