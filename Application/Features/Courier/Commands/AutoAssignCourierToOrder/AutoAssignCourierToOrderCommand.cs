using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Commands.AutoAssignCourierToOrder;

public class AutoAssignCourierToOrderCommand(int orderId) : IRequest<Response<string>>
{
    public int OrderId { get; } = orderId;
}