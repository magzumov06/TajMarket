using Domain.DTOs.CouponDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;


public class CouponController(ICouponService couponService) : BaseApiController
{

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        var res = await couponService.CreateAsync(dto);
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPut("{code}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeactivateCoupon(string code)
    {
        var res = await couponService.DeactivateAsync(code);
        return StatusCode((int)res.StatusCode, res);
    }


    
    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        var res = await couponService.ValidateAsync(request.Code, request.OrderAmount);
        return StatusCode((int)res.StatusCode, res);
    }


    
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveCoupons()
    {
        var res = await couponService.GetAllActiveAsync();
        return StatusCode((int)res.StatusCode, res);
    }
}

public record ValidateCouponRequest(string Code, decimal OrderAmount);
