using System.Net;
using Application.Common.Interfaces;
using Application.Features.Review.DTOs;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Review.Commands.CreateReview;

public class CreateReviewCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateReviewCommandHandler> logger)
    : IRequestHandler<CreateReviewCommand, Response<ReviewDto>>
{
    public async Task<Response<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

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
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken);

            if (product == null)
            {
                logger.LogWarning("Product not found {ProductId}", dto.ProductId);
                return new Response<ReviewDto>(HttpStatusCode.NotFound, "Маҳсулот ёфт нашуд");
            }

            var alreadyReviewed = await context.Reviews
                .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId, cancellationToken);

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
                    oi.Order.Status == OrderStatus.Delivered,
                    cancellationToken);

            if (!hasPurchased)
            {
                logger.LogWarning("User {UserId} has not purchased product {ProductId}", userId, dto.ProductId);
                return new Response<ReviewDto>(HttpStatusCode.BadRequest, "Шумо метавонед танҳо ба маҳсулоти харидаатон баҳо гузоред");
            }

            var review = new Domain.Entities.ReviewEntity.Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync(cancellationToken);

            await RecalculateSellerRatingHelper.RecalculateAsync(context, product.SellerProfileId, logger, cancellationToken);

            var user = await context.Users.FirstAsync(u => u.Id == userId, cancellationToken);

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
}