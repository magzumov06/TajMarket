using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Commands.DeleteCourier;

public class DeleteCourierCommand(int courierId) : IRequest<Response<string>>
{
    public int CourierId { get; } = courierId;
}