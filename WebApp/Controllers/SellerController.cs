using Application.Features.Seller.Commands.BecomeSeller;
using Application.Features.Seller.Commands.UploadSellerLogo;
using Application.Features.Seller.Queries.GetMySellerProfile;
using Application.Features.Seller.Queries.GetSellerById;
using Domain.DTOs.SellerDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class SellersController(IMediator mediator) : BaseApiController
{
    [Authorize]
    [HttpPost("become-seller")]
    public async Task<IActionResult> BecomeSeller([FromBody] SellerRegisterDto dto)
    {
        var result = await mediator.Send(new BecomeSellerCommand(UserId, dto));
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize(Roles = "Seller")]
    [HttpPost("me/logo")]
    public async Task<IActionResult> UploadLogo([FromForm] UploadStoreLogoDto dto)
    {
        if (dto.File == null)
            return BadRequest(new { message = "Файл интихоб нашудааст" });

        var result = await mediator.Send(new UploadSellerLogoCommand(UserId, dto.File));
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await mediator.Send(new GetMySellerProfileQuery(UserId));
        return StatusCode((int)result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await mediator.Send(new GetSellerByIdQuery(id));
        return StatusCode((int)result.StatusCode, result);
    }
}