using Application.Features.Auth.Commands.ResendOtp;
using Application.Features.Auth.Commands.VerifyOtp;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController(IMediator mediator) : ControllerBase
{
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
    {
        var result = await mediator.Send(new VerifyOtpCommand(dto));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("resend")]
    public async Task<IActionResult> ResendOtp([FromQuery] string email)
    {
        var result = await mediator.Send(new ResendOtpCommand(email));
        return StatusCode((int)result.StatusCode, result);
    }
}