using Application.Features.Auth.Commands.ChangePassword;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class AuthController(IMediator mediator) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await mediator.Send(new RegisterCommand(dto));
        return StatusCode((int)result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await mediator.Send(new LoginCommand(dto));
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var result = await mediator.Send(new ChangePasswordCommand(UserId, dto));
        return StatusCode((int)result.StatusCode, result);
    }
}