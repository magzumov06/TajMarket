using Domain.DTOs.UserDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
public class UsersController(IUserService userService) : BaseApiController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await userService.GetProfileAsync(UserId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        var result = await userService.UpdateProfileAsync(UserId, dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("me/avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarDto dto)
    {
        if (dto.File == null)
            return BadRequest(new { message = "Файл интихоб нашудааст" });

        var result = await userService.UploadAvatarAsync(UserId, dto.File);
        return StatusCode(result.StatusCode, result);
    }
}
