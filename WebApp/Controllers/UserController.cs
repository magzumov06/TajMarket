using Application.Features.User.Commands.UpdateProfile;
using Application.Features.User.Commands.UploadAvatar;
using Application.Features.User.DTOs;
using Application.Features.User.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
public class UsersController(IMediator mediator) : BaseApiController
{
    [HttpPost("me/avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarDto dto)
    {
        if (dto.File == null)
            return BadRequest(new { message = "Файл интихоб нашудааст" });

        var result = await mediator.Send(new UploadAvatarCommand(UserId, dto.File));
        return StatusCode((int)result.StatusCode, result);
    }
    
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        var result = await mediator.Send(new UpdateProfileCommand(UserId, dto));
        return StatusCode((int)result.StatusCode, result);
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await mediator.Send(new GetMyProfileQuery(UserId));
        return StatusCode((int)result.StatusCode, result);
    }
}