using System.Net;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, Response<AuthResponseDto>>
{
    public async Task<Response<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenValue = request.Dto.RefreshToken;

        try
        {
            var existingToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == tokenValue, cancellationToken);

            if (existingToken == null)
            {
                logger.LogWarning("Refresh token not found");
                return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, "Refresh token нодуруст аст");
            }

            if (!existingToken.IsActive)
            {
                logger.LogWarning("Refresh token is expired or revoked for user {UserId}", existingToken.UserId);
                
                if (existingToken.IsRevoked)
                {
                    var allUserTokens = await context.RefreshTokens
                        .Where(rt => rt.UserId == existingToken.UserId && rt.RevokedAt == null)
                        .ToListAsync(cancellationToken);

                    foreach (var t in allUserTokens)
                        t.RevokedAt = DateTime.UtcNow;

                    await context.SaveChangesAsync(cancellationToken);

                    logger.LogWarning(
                        "Potential token reuse detected for user {UserId} — all refresh tokens revoked",
                        existingToken.UserId);
                }

                return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, "Refresh token эътибор надорад, лутфан аз нав вуруд кунед");
            }

            var user = await identityService.FindUserByIdAsync(existingToken.UserId, cancellationToken);

            if (user == null || !user.IsActive)
            {
                logger.LogWarning("User not found or inactive for refresh token {UserId}", existingToken.UserId);
                return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, "Корбар ёфт нашуд ё ғайрифаъол аст");
            }

            var newRefreshTokenValue = tokenService.GenerateRefreshToken();

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.ReplacedByToken = newRefreshTokenValue;

            context.RefreshTokens.Add(new Domain.Entities.RefreshToken
            {
                Token = newRefreshTokenValue,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpiryDays)
            });

            var roles = await identityService.GetUserRolesAsync(user, cancellationToken);
            var (newAccessToken, expiresAt) = tokenService.GenerateToken(user, roles);

            await context.SaveChangesAsync(cancellationToken);

            var response = new AuthResponseDto(
                newAccessToken, expiresAt, newRefreshTokenValue,
                user.Id, user.FullName, user.Email ?? string.Empty, roles.ToList());

            logger.LogInformation("Access token refreshed successfully for user {UserId}", user.Id);

            return new Response<AuthResponseDto>(response, "Token навсозӣ шуд");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return new Response<AuthResponseDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}