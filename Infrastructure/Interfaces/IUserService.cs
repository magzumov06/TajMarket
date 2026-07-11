using Domain.DTOs.UserDto;
using Domain.Responses;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces;

public interface IUserService
{
    Task<Response<UserProfileDto>> GetProfileAsync(int userId);
    Task<Response<UserProfileDto>> UpdateProfileAsync(int userId, UpdateUserDto dto);
    Task<Response<string>> UploadAvatarAsync(int userId, IFormFile file);
}