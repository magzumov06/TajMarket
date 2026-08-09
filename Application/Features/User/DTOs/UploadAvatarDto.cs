using Microsoft.AspNetCore.Http;

namespace Application.Features.User.DTOs;

public class UploadAvatarDto
{
    public IFormFile? File { get; set; }
}