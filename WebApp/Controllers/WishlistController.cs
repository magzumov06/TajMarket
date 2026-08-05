using Application.Features.Wishlist.Commands.AddToWishlist;
using Application.Features.Wishlist.Commands.RemoveFromWishlist;
using Application.Features.Wishlist.Queries.GetWishlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
public class WishlistController(IMediator mediator) : BaseApiController
{
    [HttpPost("products/{productId}")]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var res = await mediator.Send(new AddToWishlistCommand(UserId, productId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpDelete("products/{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var res = await mediator.Send(new RemoveFromWishlistCommand(UserId, productId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var res = await mediator.Send(new GetWishlistQuery(UserId));
        return StatusCode((int)res.StatusCode, res);
    }
}