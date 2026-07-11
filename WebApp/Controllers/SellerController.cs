using Domain.DTOs.SellerDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class SellersController(ISellerService sellerService) : BaseApiController
{
    [Authorize]
    [HttpPost("become-seller")]
    public async Task<IActionResult> BecomeSeller([FromBody] SellerRegisterDto dto)
    {
        var result = await sellerService.BecomeSellerAsync(UserId, dto);
        return StatusCode(result.StatusCode,result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await sellerService.GetProfileAsync(UserId);
        return StatusCode(result.StatusCode,result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await sellerService.GetByIdAsync(id);
        return StatusCode(result.StatusCode,result);
    }

    [Authorize(Roles = "Seller")]
    [HttpPost("me/logo")]
    public async Task<IActionResult> UploadLogo([FromForm] UploadStoreLogoDto dto)
    {
        if (dto.File == null)
            return BadRequest(new { message = "Файл интихоб нашудааст" });

        var result = await sellerService.UploadLogoAsync(UserId, dto.File);
        return StatusCode(result.StatusCode,result);
    }
}
