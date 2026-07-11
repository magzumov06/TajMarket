using System.Net;
using Domain.DTOs.UserDto;
using Domain.Entities.UserEntity;
using Domain.Responses;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class UserService(
    UserManager<User> userManager,
    IFileStorageService fileStorage) : IUserService
{
    public async Task<Response<UserProfileDto>> GetProfileAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new Response<UserProfileDto>(HttpStatusCode.NotFound,"Корбар ёфт нашуд");

        var roles = await userManager.GetRolesAsync(user);
        return new Response<UserProfileDto>(ToDto(user, roles));
    }

    public async Task<Response<UserProfileDto>> UpdateProfileAsync(int userId, UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new Response<UserProfileDto>(HttpStatusCode.NotFound,"Корбар ёфт нашуд");

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            user.PhoneNumber = dto.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var res = string.Join("; ", result.Errors.Select(e => e.Description));
            return new Response<UserProfileDto>(HttpStatusCode.BadRequest, res);
        }

        var roles = await userManager.GetRolesAsync(user);
        return new Response<UserProfileDto>(ToDto(user, roles), "Профил навсозӣ шуд");
    }

    public async Task<Response<string>> UploadAvatarAsync(int userId, Microsoft.AspNetCore.Http.IFormFile file)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new Response<string>(HttpStatusCode.NotFound,"Корбар ёфт нашуд");

        if (!string.IsNullOrEmpty(user.AvatarPublicId))
            await fileStorage.DeleteImageAsync(user.AvatarPublicId);

        var uploaded = await fileStorage.UploadImageAsync(file, "avatars");

        user.AvatarUrl = uploaded.Url;
        user.AvatarPublicId = uploaded.PublicId;
        await userManager.UpdateAsync(user);

        return new Response<string>(uploaded.Url, "Расми профил нав шуд");
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
