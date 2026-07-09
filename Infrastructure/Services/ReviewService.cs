using System.Net;
using Domain.DTOs.ReviewDtos;
using Domain.Entities.ReviewEntity;
using Domain.Enums;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ReviewService(DataContext context) : IReviewService
{
    #region MyRegion

    public async Task<Response<ReviewDto>> CreateAsync(int userId, CreateReviewDto dto)
    {
        try
        {
            if (dto.Rating is < 1 or > 5)
                return new Response<ReviewDto>(HttpStatusCode.BadRequest,"Баҳо бояд аз 1 то 5 бошад");

            var product = await context.Products
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                return new Response<ReviewDto>(HttpStatusCode.NotFound,"Маҳсулот ёфт нашуд");

            var alreadyReviewed = await context.Reviews.AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);
            if (alreadyReviewed)
                return new Response<ReviewDto>(HttpStatusCode.BadRequest,"Шумо аллакай ба ин маҳсулот баҳо додаед");

            // Танҳо касоне, ки маҳсулотро харида ва гирифтаанд (Delivered), метавонанд шарҳ гузоранд
            var hasPurchased = await context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.ProductId == dto.ProductId
                                && oi.Order.UserId == userId
                                && oi.Order.Status == OrderStatus.Delivered);

            if (!hasPurchased)
                return new Response<ReviewDto>(HttpStatusCode.BadRequest,"Шумо метавонед танҳо ба маҳсулоти харидаатон баҳо гузоред");

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

            await RecalculateSellerRatingAsync(product.SellerProfileId);

            var user = await context.Users.FirstAsync(u => u.Id == userId);
            var result = new ReviewDto(review.Id, user.FullName, user.AvatarUrl, review.Rating, review.Comment, review.CreatedAt);

            return new Response<ReviewDto>(result, "Ташаккур барои шарҳ!");
        }
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion

    #region Delete

    public async Task<Response<string>> DeleteAsync(int userId, int reviewId)
    {
        try
        {
            var review = await context.Reviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return new Response<string>(HttpStatusCode.NotFound,"Шарҳ ёфт нашуд");

            if (review.UserId != userId)
                return new Response<string>(HttpStatusCode.BadRequest,"Шумо ин шарҳро нест карда наметавонед");

            var sellerProfileId = review.Product.SellerProfileId;

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();

            await RecalculateSellerRatingAsync(sellerProfileId);

            return new Response<string>(HttpStatusCode.OK,"Шарҳ нест шуд");        }
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion

    #region GetByProduct

    public async Task<Response<List<ReviewDto>>> GetByProductAsync(int productId)
    {
        try
        {
            var reviews = await context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var res = reviews
                .Select(r => new ReviewDto(r.Id, r.User.FullName, r.User.AvatarUrl, r.Rating, r.Comment, r.CreatedAt))
                .ToList();
            return new Response<List<ReviewDto>>(res);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    #endregion
    
    private async Task RecalculateSellerRatingAsync(int sellerProfileId)
    {
        var sellerProfile = await context.SellerProfiles.FirstOrDefaultAsync(sp => sp.Id == sellerProfileId);
        if (sellerProfile == null)
            return;

        var ratings = await context.Reviews
            .Where(r => r.Product.SellerProfileId == sellerProfileId)
            .Select(r => r.Rating)
            .ToListAsync();

        sellerProfile.Rating = ratings.Count != 0 ? decimal.Round((decimal)ratings.Average(), 1) : 0;
        await context.SaveChangesAsync();
    }
}