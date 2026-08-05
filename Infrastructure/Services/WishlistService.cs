using System.Net;
using Application.Features.Wishlist.DTOs;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class WishlistService(
    DataContext context,
    ILogger<WishlistService> logger) : IWishlistService
{
    #region Add

    public async Task<Response<string>> AddAsync(
        int userId,
        int productId)
    {
        try
        {
            logger.LogInformation(
                "Adding product {ProductId} to wishlist for user {UserId}",
                productId,
                userId);


            var productExists = await context.Products
                .AnyAsync(p =>
                    p.Id == productId &&
                    p.IsActive);


            if (!productExists)
            {
                logger.LogWarning(
                    "Product not found or inactive {ProductId}",
                    productId);

                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "Маҳсулот ёфт нашуд");
            }


            var alreadyAdded = await context.WishlistItems
                .AnyAsync(w =>
                    w.UserId == userId &&
                    w.ProductId == productId);


            if (alreadyAdded)
            {
                logger.LogWarning(
                    "Product {ProductId} already exists in wishlist for user {UserId}",
                    productId,
                    userId);

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Ин маҳсулот аллакай дар рӯйхати дӯстдоштаҳо ҳаст");
            }


            context.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });


            await context.SaveChangesAsync();


            logger.LogInformation(
                "Product {ProductId} added to wishlist for user {UserId}",
                productId,
                userId);


            return new Response<string>(
                HttpStatusCode.OK,
                "Ба рӯйхати дӯстдоштаҳо илова шуд");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error adding product {ProductId} to wishlist for user {UserId}",
                productId,
                userId);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion


    #region Remove

    public async Task<Response<string>> RemoveAsync(
        int userId,
        int productId)
    {
        try
        {
            logger.LogInformation(
                "Removing product {ProductId} from wishlist for user {UserId}",
                productId,
                userId);


            var item = await context.WishlistItems
                .FirstOrDefaultAsync(w =>
                    w.UserId == userId &&
                    w.ProductId == productId);


            if (item == null)
            {
                logger.LogWarning(
                    "Wishlist item not found product {ProductId} user {UserId}",
                    productId,
                    userId);

                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "Ин маҳсулот дар рӯйхати дӯстдоштаҳо нест");
            }


            context.WishlistItems.Remove(item);

            await context.SaveChangesAsync();


            logger.LogInformation(
                "Product {ProductId} removed from wishlist for user {UserId}",
                productId,
                userId);


            return new Response<string>(
                HttpStatusCode.OK,
                "Аз рӯйхати дӯстдоштаҳо бароварда шуд");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error removing product {ProductId} from wishlist for user {UserId}",
                productId,
                userId);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion


    #region GetAll

    public async Task<Response<List<WishlistItemDto>>> GetAllAsync(
        int userId)
    {
        try
        {
            logger.LogInformation(
                "Retrieving wishlist for user {UserId}",
                userId);


            var items = await context.WishlistItems
                .AsNoTracking()
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();


            var result = items
                .Select(w => new WishlistItemDto(
                    w.Id,
                    w.ProductId,
                    w.Product.Name,
                    w.Product.Images
                        .FirstOrDefault(i => i.IsMain)?.Url
                        ?? w.Product.Images.FirstOrDefault()?.Url,
                    w.Product.DiscountPrice
                        ?? w.Product.Price,
                    w.AddedAt))
                .ToList();


            logger.LogInformation(
                "Retrieved {WishlistCount} wishlist items for user {UserId}",
                result.Count,
                userId);


            return new Response<List<WishlistItemDto>>(result);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error retrieving wishlist for user {UserId}",
                userId);


            return new Response<List<WishlistItemDto>>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion
}