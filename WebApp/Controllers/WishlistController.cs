using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;


public class WishlistController(IWishlistService wishlistService) : BaseApiController
{
    
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
