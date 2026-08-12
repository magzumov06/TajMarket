using Domain.Responses;
using MediatR;

namespace Application.Features.User.Commands.BlockUser;

public class BlockUserCommand(int userId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
}