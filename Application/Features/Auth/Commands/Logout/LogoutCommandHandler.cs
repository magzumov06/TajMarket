using System.Net;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IApplicationDbContext context,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Response<string>>
{
    public async Task<Response<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            context.RevokedTokens.Add(new RevokedToken
            {
                Jti = request.Jti,
                ExpiresAt = request.TokenExpiresAt
            });

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Token revoked (logout) {Jti}", request.Jti);

            return new Response<string>(HttpStatusCode.OK, "Аз ҳисоб баромадед");
        }
        catch (Exception e)
        {
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}