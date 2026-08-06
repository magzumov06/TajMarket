using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cart;

internal static class CartHelper
{
    public static async Task<Domain.Entities.CartEntity.Cart> GetOrCreateCartAsync(
        IApplicationDbContext context, int userId, ILogger logger, CancellationToken cancellationToken)
    {
        var cart = await context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart != null)
            return cart;

        cart = new Domain.Entities.CartEntity.Cart
        {
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        };

        context.Carts.Add(cart);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("New cart created for user {UserId}", userId);

        return cart;
    }
}