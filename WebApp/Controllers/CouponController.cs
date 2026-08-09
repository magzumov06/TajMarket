using Application.Features.Coupon.Commands.CreateCoupon;
using Application.Features.Coupon.Commands.DeactivateCoupon;
using Application.Features.Coupon.DTOs;
using Application.Features.Coupon.Queries.GetAllActiveCoupons;
using Application.Features.Coupon.Queries.ValidateCoupon;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class CouponController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        var res = await mediator.Send(new CreateCouponCommand(dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        var res = await mediator.Send(new ValidateCouponQuery(request.Code, request.OrderAmount));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPut("{code}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateCoupon(string code)
    {
        var res = await mediator.Send(new DeactivateCouponCommand(code));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveCoupons()
    {
        var res = await mediator.Send(new GetAllActiveCouponsQuery());
        return StatusCode((int)res.StatusCode, res);
    }
}

public record ValidateCouponRequest(string Code, decimal OrderAmount);