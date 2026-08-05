using System.Net;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Wishlist.Commands.AddToWishlist;

public class AddToWishlistCommandHandler(
    IApplicationDbContext context,
    ILogger<AddToWishlistCommandHandler> logger)
    : IRequestHandler<AddToWishlistCommand, Response<string>>
{
    public async Task<Response<string>> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var (userId, productId) = (request.UserId, request.ProductId);

        try
        {
            logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}", productId, userId);

            var productExists = await context.Products
                .AnyAsync(p => p.Id == productId && p.IsActive, cancellationToken);

            if (!productExists)
            {
                logger.LogWarning("Product not found or inactive {ProductId}", productId);
                return new Response<string>(HttpStatusCode.NotFound, "Маҳсулот ёфт нашуд");
            }

            var alreadyAdded = await context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

            if (alreadyAdded)
            {
                logger.LogWarning("Product {ProductId} already exists in wishlist for user {UserId}", productId, userId);
                return new Response<string>(HttpStatusCode.BadRequest, "Ин маҳсулот аллакай дар рӯйхати дӯстдоштаҳо ҳаст");
            }

            context.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product {ProductId} added to wishlist for user {UserId}", productId, userId);

            return new Response<string>(HttpStatusCode.OK, "Ба рӯйхати дӯстдоштаҳо илова шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding product {ProductId} to wishlist for user {UserId}", productId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}