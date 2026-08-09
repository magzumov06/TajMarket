using System.Net;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, Response<AuthResponseDto>>
{
    private const string GenericError = "Почтаи электронӣ ё парол нодуруст аст";

    public async Task<Response<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        dto.Email = dto.Email.Trim().ToLowerInvariant();

        try
        {
            logger.LogInformation("Login attempt for {Email}", dto.Email);

            var user = await identityService.FindUserByEmailAsync(dto.Email, cancellationToken);

            if (user == null || !user.IsActive)
            {
                logger.LogWarning("Login failed. User not found or inactive");
                return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, GenericError);
            }

            if (!await identityService.IsEmailConfirmedAsync(user, cancellationToken))
                return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, "Почтаи электронии шумо тасдиқ карда нашудааст.");

            if (await identityService.IsLockedOutAsync(user, cancellationToken))
                return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, "Ҳисоби шумо қулф шудааст.");

            var passwordValid = await identityService.CheckPasswordAsync(user, dto.Password, cancellationToken);

            if (!passwordValid)
            {
                await identityService.RecordAccessFailureAsync(user, cancellationToken);
                logger.LogWarning("Wrong password for user {UserId}", user.Id);
                return new Response<AuthResponseDto>(HttpStatusCode.Unauthorized, GenericError);
            }

            await identityService.ResetAccessFailedCountAsync(user, cancellationToken);

            var roles = await identityService.GetUserRolesAsync(user, cancellationToken);

            var (token, expiresAt) = tokenService.GenerateToken(user, roles);

            var response = new AuthResponseDto(token, expiresAt, user.Id, user.FullName, user.Email ?? string.Empty, roles.ToList());

            logger.LogInformation("Login successful {UserId}", user.Id);

            return new Response<AuthResponseDto>(response, "Вуруд бомуваффақият анҷом ёфт.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login error");
            return new Response<AuthResponseDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}