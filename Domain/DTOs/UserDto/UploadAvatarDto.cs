using Microsoft.AspNetCore.Http;

namespace Domain.DTOs.UserDto;

public class UploadAvatarDto
{
    public IFormFile? File { get; set; }
}