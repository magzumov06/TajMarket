using Application.Features.Cart.Commands.AddToCart;
using Application.Features.Cart.Commands.ClearCart;
using Application.Features.Cart.Commands.RemoveFromCart;
using Application.Features.Cart.Commands.UpdateCartItem;
using Application.Features.Cart.DTOs;
using Application.Features.Cart.Queries.GetCartItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]  
public class CartController(IMediator mediator) : BaseApiController
{
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var res = await mediator.Send(new AddToCartCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        var res = await mediator.Send(new UpdateCartItemCommand(UserId, cartItemId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var res = await mediator.Send(new RemoveFromCartCommand(UserId, cartItemId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet]
    public async Task<IActionResult> GetCartItems()
    {
        var res = await mediator.Send(new GetCartItemsQuery(UserId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var res = await mediator.Send(new ClearCartCommand(UserId));
        return StatusCode((int)res.StatusCode, res);
    }
}