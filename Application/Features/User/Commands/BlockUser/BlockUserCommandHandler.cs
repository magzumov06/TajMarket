using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.User.Commands.BlockUser;

public class BlockUserCommandHandler(
    IIdentityService identityService,
    ILogger<BlockUserCommandHandler> logger)
    : IRequestHandler<BlockUserCommand, Response<string>>
{
    public async Task<Response<string>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        try
        {
            logger.LogInformation("Blocking user {UserId}", userId);

            var user = await identityService.FindUserByIdAsync(userId, cancellationToken);

            if (user == null)
                return new Response<string>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");

            if (!user.IsActive)
                return new Response<string>(HttpStatusCode.BadRequest, "Ин корбар аллакай баста шудааст");

            user.IsActive = false;
            user.TokenValidFrom = DateTime.UtcNow;  
            
            var (succeeded, errors) = await identityService.UpdateUserAsync(user, cancellationToken);

            if (!succeeded)
                return new Response<string>(HttpStatusCode.BadRequest, string.Join("; ", errors));

            logger.LogInformation("User {UserId} blocked successfully", userId);

            return new Response<string>(HttpStatusCode.OK, "Корбар баста шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while blocking user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}