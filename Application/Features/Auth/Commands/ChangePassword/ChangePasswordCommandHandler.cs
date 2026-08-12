using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IIdentityService identityService,
    ILogger<ChangePasswordCommandHandler> logger)
    : IRequestHandler<ChangePasswordCommand, Response<string>>
{
    public async Task<Response<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("Changing password for {UserId}", userId);

            var user = await identityService.FindUserByIdAsync(userId, cancellationToken);

            if (user == null)
                return new Response<string>(HttpStatusCode.NotFound, "User not found");

            var (succeeded, _) = await identityService.ChangePasswordAsync(user, dto.OldPassword, dto.Password, cancellationToken);

            if (!succeeded)
                return new Response<string>(HttpStatusCode.BadRequest, "Your password not changed");
            
            user.TokenValidFrom = DateTime.UtcNow;
            await identityService.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Password changed successfully for user {UserId}", userId);

            return new Response<string>(HttpStatusCode.OK, "Паролатон иваз шуд. Лутфан аз нав вуруд кунед.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Password change error {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}