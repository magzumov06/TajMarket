using System.Net;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IApplicationDbContext context,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Response<string>>
{
    public async Task<Response<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        context.RevokedTokens.Add(new RevokedToken
        {
            Jti = request.Jti,
            ExpiresAt = request.AccessTokenExpiresAt
        });

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var refreshToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (refreshToken is { RevokedAt: null })
                refreshToken.RevokedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Logout completed, token revoked {Jti}", request.Jti);

        return new Response<string>(HttpStatusCode.OK, "Аз ҳисоб баромадед");
    }
}