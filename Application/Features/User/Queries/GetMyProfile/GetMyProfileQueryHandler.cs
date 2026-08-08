// Application/Features/User/Queries/GetMyProfile/GetMyProfileQueryHandler.cs
using System.Net;
using Application.Common.Interfaces;
using Domain.DTOs.UserDto;
using Domain.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.User.Queries.GetMyProfile;

public class GetMyProfileQueryHandler(
    IIdentityService identityService,
    ILogger<GetMyProfileQueryHandler> logger)
    : IRequestHandler<GetMyProfileQuery, Response<UserProfileDto>>
{
    public async Task<Response<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Getting profile for user {UserId}", userId);

            var user = await identityService.FindUserByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", userId);
                return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }

            var roles = await identityService.GetUserRolesAsync(user, cancellationToken);

            logger.LogInformation("Profile retrieved successfully for user {UserId}", userId);

            return new Response<UserProfileDto>(UserMapper.ToDto(user, roles));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting profile for user {UserId}", userId);
            return new Response<UserProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}