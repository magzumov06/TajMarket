using System.Net;
using Application.Common.Interfaces;
using Application.Features.User.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.User.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    IIdentityService identityService,
    ILogger<UpdateProfileCommandHandler> logger)
    : IRequestHandler<UpdateProfileCommand, Response<UserProfileDto>>
{
    public async Task<Response<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("Updating profile for user {UserId}", userId);

            var user = await identityService.FindUserByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", userId);
                return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            var (succeeded, errors) = await identityService.UpdateUserAsync(user, cancellationToken);

            if (!succeeded)
            {
                var errorMessage = string.Join("; ", errors);
                logger.LogWarning("Failed updating profile for user {UserId}: {Errors}", userId, errorMessage);
                return new Response<UserProfileDto>(HttpStatusCode.BadRequest, errorMessage);
            }

            var roles = await identityService.GetUserRolesAsync(user, cancellationToken);

            logger.LogInformation("Profile updated successfully for user {UserId}", userId);

            return new Response<UserProfileDto>(UserMapper.ToDto(user, roles), "Профил навсозӣ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating profile for user {UserId}", userId);
            return new Response<UserProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}