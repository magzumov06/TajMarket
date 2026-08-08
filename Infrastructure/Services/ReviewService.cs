using System.Net;
using Application.Features.Review.DTOs;
using Domain.Entities.ReviewEntity;
using Domain.Enums;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ReviewService(
    DataContext context,
    ILogger<ReviewService> logger) : IReviewService
{
    #region Create

    public async Task<Response<ReviewDto>> CreateAsync(int userId, CreateReviewDto dto)
    {
        try
        {
            logger.LogInformation("Creating review for product {ProductId} by user {UserId}", dto.ProductId, userId);

            if (dto.Rating is < 1 or > 5)
            {
                logger.LogWarning("Invalid rating {Rating} for product {ProductId}", dto.Rating, dto.ProductId);

                return new Response<ReviewDto>(HttpStatusCode.BadRequest, "Баҳо бояд аз 1 то 5 бошад");
            }


            var product = await context.Products
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);


            if (product == null)
            {
                logger.LogWarning("Product not found {ProductId}", dto.ProductId);

                return new Response<ReviewDto>(HttpStatusCode.NotFound, "Маҳсулот ёфт нашуд");
            }


            var alreadyReviewed = await context.Reviews
                .AnyAsync(r =>
                    r.ProductId == dto.ProductId &&
                    r.UserId == userId);


            if (alreadyReviewed)
            {
                logger.LogWarning("User {UserId} already reviewed product {ProductId}", userId, dto.ProductId);

                return new Response<ReviewDto>(HttpStatusCode.BadRequest, "Шумо аллакай ба ин маҳсулот баҳо додаед");
            }


            var hasPurchased = await context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi =>
                    oi.ProductId == dto.ProductId &&
                    oi.Order.UserId == userId &&
                    oi.Order.Status == OrderStatus.Delivered);


            if (!hasPurchased)
            {
                logger.LogWarning("User {UserId} has not purchased product {ProductId}", userId, dto.ProductId);

                return new Response<ReviewDto>(HttpStatusCode.BadRequest, "Шумо метавонед танҳо ба маҳсулоти харидаатон баҳо гузоред");
            }


            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };


            context.Reviews.Add(review);

            await context.SaveChangesAsync();


            await RecalculateSellerRatingAsync(
                product.SellerProfileId);


            var user = await context.Users
                .FirstAsync(u => u.Id == userId);


            var result = new ReviewDto(
                review.Id,
                user.FullName,
                user.AvatarUrl,
                review.Rating,
                review.Comment,
                review.CreatedAt);
            
            logger.LogInformation("Review created successfully {ReviewId}", review.Id);


            return new Response<ReviewDto>(result, "Ташаккур барои шарҳ!");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating review for product {ProductId} user {UserId}", dto.ProductId, userId);


            return new Response<ReviewDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion


    #region Delete

    public async Task<Response<string>> DeleteAsync(int userId, int reviewId)
    {
        try
        {
            logger.LogInformation("Deleting review {ReviewId} by user {UserId}", reviewId, userId);
            
            var review = await context.Reviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r =>
                    r.Id == reviewId);


            if (review == null)
            {
                logger.LogWarning("Review not found {ReviewId}", reviewId);

                return new Response<string>(HttpStatusCode.NotFound, "Шарҳ ёфт нашуд");
            }


            if (review.UserId != userId)
            {
                logger.LogWarning("User {UserId} tried to delete another user's review {ReviewId}", userId, reviewId);

                return new Response<string>(HttpStatusCode.BadRequest, "Шумо ин шарҳро нест карда наметавонед");
            }


            var sellerProfileId =
                review.Product.SellerProfileId;


            context.Reviews.Remove(review);

            await context.SaveChangesAsync();


            await RecalculateSellerRatingAsync(
                sellerProfileId);


            logger.LogInformation("Review deleted successfully {ReviewId}", reviewId);

            return new Response<string>(HttpStatusCode.OK, "Шарҳ нест шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting review {ReviewId}", reviewId);
            
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion


    #region GetByProduct

    public async Task<Response<List<ReviewDto>>> GetByProductAsync(int productId)
    {
        try
        {
            logger.LogInformation("Retrieving reviews for product {ProductId}", productId);

            var reviews = await context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();


            var result = reviews
                .Select(r => new ReviewDto(
                    r.Id,
                    r.User.FullName,
                    r.User.AvatarUrl,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt))
                .ToList();


            logger.LogInformation("Retrieved {ReviewCount} reviews for product {ProductId}", result.Count, productId);

            return new Response<List<ReviewDto>>(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving reviews for product {ProductId}", productId);

            return new Response<List<ReviewDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion


    private async Task RecalculateSellerRatingAsync(int sellerProfileId)
    {
        try
        {
            logger.LogInformation("Recalculating seller rating {SellerProfileId}", sellerProfileId);

            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp =>
                    sp.Id == sellerProfileId);
            
            if (sellerProfile == null)
            {
                logger.LogWarning("Seller profile not found {SellerProfileId}", sellerProfileId);

                return;
            }


            var ratings = await context.Reviews
                .Where(r =>
                    r.Product.SellerProfileId == sellerProfileId)
                .Select(r => r.Rating)
                .ToListAsync();


            sellerProfile.Rating =
                ratings.Count != 0
                    ? decimal.Round(
                        (decimal)ratings.Average(),
                        1)
                    : 0;
            
            await context.SaveChangesAsync();
            
            logger.LogInformation("Seller rating updated {SellerProfileId} Rating {Rating}", sellerProfileId, sellerProfile.Rating);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error recalculating seller rating {SellerProfileId}", sellerProfileId);
        }
    }
}