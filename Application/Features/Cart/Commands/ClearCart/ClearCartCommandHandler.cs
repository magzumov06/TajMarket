using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cart.Commands.ClearCart;

public class ClearCartCommandHandler(
    IApplicationDbContext context,
    ILogger<ClearCartCommandHandler> logger)
    : IRequestHandler<ClearCartCommand, Response<string>>
{
    public async Task<Response<string>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Clearing cart for user {UserId}", userId);

            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart?.Items != null && cart.Items.Count != 0)
            {
                context.CartItems.RemoveRange(cart.Items);
                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Cart cleared for user {UserId}", userId);

            return new Response<string>(HttpStatusCode.OK, "Cart cleared");
        }
        catch (Exception e)
        {
            logger.LogError(e, "ClearCartAsync failed for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}