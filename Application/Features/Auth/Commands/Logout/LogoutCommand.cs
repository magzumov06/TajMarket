using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommand(string jti, DateTime tokenExpiresAt) : IRequest<Response<string>>
{
    public string Jti { get; } = jti;
    public DateTime TokenExpiresAt { get; } = tokenExpiresAt;
}