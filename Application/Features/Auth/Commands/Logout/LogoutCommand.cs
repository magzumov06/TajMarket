using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommand(string jti, DateTime accessTokenExpiresAt, string? refreshToken) : IRequest<Response<string>>
{
    public string Jti { get; } = jti;
    public DateTime AccessTokenExpiresAt { get; } = accessTokenExpiresAt;
    public string? RefreshToken { get; } = refreshToken;
}