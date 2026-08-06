using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartCommandHandler(
    IApplicationDbContext context,
    ILogger<RemoveFromCartCommandHandler> logger)
    : IRequestHandler<RemoveFromCartCommand, Response<string>>
{
    public async Task<Response<string>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var (userId, cartItemId) = (request.UserId, request.CartItemId);

        try
        {
            logger.LogInformation("Removing cart item {CartItemId} for user {UserId}", cartItemId, userId);

            var item = await context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(
                    ci =>
                        ci.Id == cartItemId &&
                        ci.Cart.UserId == userId,
                    cancellationToken);

            if (item == null)
            {
                logger.LogWarning("Cart item not found {CartItemId} for user {UserId}", cartItemId, userId);
                return new Response<string>(HttpStatusCode.NotFound, "Ин ашё дар сабад ёфт нашуд");
            }

            var cartId = item.CartId;

            context.CartItems.Remove(item);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Cart item {CartItemId} removed from cart {CartId}", cartItemId, cartId);

            return new Response<string>(HttpStatusCode.OK, "Item removed");
        }
        catch (Exception e)
        {
            logger.LogError(e, "RemoveFromCartAsync failed for cart item {CartItemId} and user {UserId}", cartItemId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}