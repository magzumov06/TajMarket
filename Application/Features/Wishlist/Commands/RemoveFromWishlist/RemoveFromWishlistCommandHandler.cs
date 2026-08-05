using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler(
    IApplicationDbContext context,
    ILogger<RemoveFromWishlistCommandHandler> logger)
    : IRequestHandler<RemoveFromWishlistCommand, Response<string>>
{
    public async Task<Response<string>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var (userId, productId) = (request.UserId, request.ProductId);

        try
        {
            logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}", productId, userId);

            var item = await context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

            if (item == null)
            {
                logger.LogWarning("Wishlist item not found product {ProductId} user {UserId}", productId, userId);
                return new Response<string>(HttpStatusCode.NotFound, "Ин маҳсулот дар рӯйхати дӯстдоштаҳо нест");
            }

            context.WishlistItems.Remove(item);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product {ProductId} removed from wishlist for user {UserId}", productId, userId);

            return new Response<string>(HttpStatusCode.OK, "Аз рӯйхати дӯстдоштаҳо бароварда шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error removing product {ProductId} from wishlist for user {UserId}", productId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}