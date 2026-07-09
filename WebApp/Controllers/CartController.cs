using Domain.DTOs.CartDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController(ICartService cartService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");


    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var res = await cartService.AddToCartAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        var res = await cartService.UpdateCartItemAsync(UserId, cartItemId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var res = await cartService.RemoveFromCartAsync(UserId, cartItemId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet]
    public async Task<IActionResult> GetCartItems()
    {
        var res = await cartService.GetCartItemsAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var res = await cartService.ClearCartAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }
}