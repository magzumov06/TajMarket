using Domain.Responses;
using MediatR;

namespace Application.Features.User.Commands.UnblockUser;

public class UnblockUserCommand(int userId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
}