using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController(IWishlistService wishlistService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");


    [HttpPost("products/{productId}")]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var res = await wishlistService.AddAsync(UserId, productId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpDelete("products/{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var res = await wishlistService.RemoveAsync(UserId, productId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var res = await wishlistService.GetAllAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }
}
