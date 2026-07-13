using Domain.DTOs.AuthDto;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController(IOtpService otpService) : ControllerBase
{

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp(
        VerifyOtpDto dto)
    {
        var result = await otpService.VerifyOtpAsync(dto);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }



    [HttpPost("resend")]
    public async Task<IActionResult> ResendOtp(
        [FromQuery] string email)
    {
        var result = await otpService.ResendOtpAsync(email);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }
}