using System.Net;
using Domain.DTOs.UserDto;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class UserService(
    UserManager<User> userManager,
    IFileStorageService fileStorage,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Response<UserProfileDto>> GetProfileAsync(int userId)
    {
        try
        {
            logger.LogInformation("Getting profile for user {UserId}", userId);

            var user = await userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", userId);

                return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }

            var roles = await userManager.GetRolesAsync(user);

            logger.LogInformation("Profile retrieved successfully for user {UserId}", userId);
            
            return new Response<UserProfileDto>(ToDto(user, roles));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting profile for user {UserId}", userId);

            return new Response<UserProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<UserProfileDto>> UpdateProfileAsync(int userId, UpdateUserDto dto)
    {
        try
        {
            logger.LogInformation("Updating profile for user {UserId}", userId);

            var user = await userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", userId);

                return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }


            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;


            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;


            var result = await userManager.UpdateAsync(user);


            if (!result.Succeeded)
            {
                var res = string.Join("; ", result.Errors.Select(e => e.Description));

                logger.LogWarning("Failed updating profile for user {UserId}: {Errors}", userId, res);

                return new Response<UserProfileDto>(HttpStatusCode.BadRequest, res);
            }
            
            var roles = await userManager.GetRolesAsync(user);
            
            logger.LogInformation("Profile updated successfully for user {UserId}", userId);

            return new Response<UserProfileDto>(ToDto(user, roles), "Профил навсозӣ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating profile for user {UserId}", userId);

            return new Response<UserProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<string>> UploadAvatarAsync(int userId, IFormFile file)
    {
        try
        {
            logger.LogInformation("Uploading avatar for user {UserId}", userId);

            var user = await userManager.FindByIdAsync(userId.ToString());
            
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

            await userManager.UpdateAsync(user);

            logger.LogInformation("Avatar uploaded successfully for user {UserId}", userId);

            return new Response<string>(uploaded.Url, "Расми профил нав шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error uploading avatar for user {UserId}", userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    private static UserProfileDto ToDto(User user, IList<string> roles) => new(
            user.Id,
            user.FullName,
            user.Email!,
            user.PhoneNumber,
            user.AvatarUrl,
            roles.ToList()
        );
}