// Application/Features/User/Commands/UploadAvatar/UploadAvatarCommandHandler.cs
using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.User.Commands.UploadAvatar;

public class UploadAvatarCommandHandler(
    IIdentityService identityService,
    IFileStorageService fileStorage,
    ILogger<UploadAvatarCommandHandler> logger)
    : IRequestHandler<UploadAvatarCommand, Response<string>>
{
    public async Task<Response<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var (userId, file) = (request.UserId, request.File);

        try
        {
            logger.LogInformation("Uploading avatar for user {UserId}", userId);

            var user = await identityService.FindUserByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", userId);
                return new Response<string>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }

            if (!string.IsNullOrEmpty(user.AvatarPublicId))
            {
                logger.LogInformation("Deleting old avatar {AvatarPublicId} for user {UserId}", user.AvatarPublicId, userId);
                await fileStorage.DeleteImageAsync(user.AvatarPublicId);
            }

            var uploaded = await fileStorage.UploadImageAsync(file, "avatars");

            user.AvatarUrl = uploaded.Url;
            user.AvatarPublicId = uploaded.PublicId;

            await identityService.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Avatar uploaded successfully for user {UserId}", userId);

            return new Response<string>(uploaded.Url, "Расми профил нав шуд");
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Invalid avatar file for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.BadRequest, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error uploading avatar for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}