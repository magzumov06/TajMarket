using System.Net;
using Application.Common.Interfaces;
using Application.Features.Wishlist.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Wishlist.Queries.GetWishlist;

public class GetWishlistQueryHandler(
    IApplicationDbContext context,
    ILogger<GetWishlistQueryHandler> logger)
    : IRequestHandler<GetWishlistQuery, Response<List<WishlistItemDto>>>
{
    public async Task<Response<List<WishlistItemDto>>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Retrieving wishlist for user {UserId}", userId);

            var items = await context.WishlistItems
                .AsNoTracking()
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync(cancellationToken);

            var result = items
                .Select(w => new WishlistItemDto(
                    w.Id,
                    w.ProductId,
                    w.Product.Name,
                    w.Product.Images.FirstOrDefault(i => i.IsMain)?.Url
                        ?? w.Product.Images.FirstOrDefault()?.Url,
                    w.Product.DiscountPrice ?? w.Product.Price,
                    w.AddedAt))
                .ToList();

            logger.LogInformation("Retrieved {WishlistCount} wishlist items for user {UserId}", result.Count, userId);

            return new Response<List<WishlistItemDto>>(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving wishlist for user {UserId}", userId);
            return new Response<List<WishlistItemDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}