using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.User.Commands.UnblockUser;

public class UnblockUserCommandHandler(
    IIdentityService identityService,
    ILogger<UnblockUserCommandHandler> logger)
    : IRequestHandler<UnblockUserCommand, Response<string>>
{
    public async Task<Response<string>> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Unblocking user {UserId}", userId);

            var user = await identityService.FindUserByIdAsync(userId, cancellationToken);

            if (user == null)
                return new Response<string>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");

            if (user.IsActive)
                return new Response<string>(HttpStatusCode.BadRequest, "Ин корбар аллакай фаъол аст");

            user.IsActive = true;

            var (succeeded, errors) = await identityService.UpdateUserAsync(user, cancellationToken);

            if (!succeeded)
                return new Response<string>(HttpStatusCode.BadRequest, string.Join("; ", errors));

            logger.LogInformation("User {UserId} unblocked successfully", userId);

            return new Response<string>(HttpStatusCode.OK, "Корбар боз фаъол шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unblocking user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}