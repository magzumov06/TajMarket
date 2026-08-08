using System.Net;
using Application.Common.Interfaces;
using Application.Features.Review.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Review.Queries.GetReviewsByProduct;

public class GetReviewsByProductQueryHandler(
    IApplicationDbContext context,
    ILogger<GetReviewsByProductQueryHandler> logger)
    : IRequestHandler<GetReviewsByProductQuery, Response<List<ReviewDto>>>
{
    public async Task<Response<List<ReviewDto>>> Handle(GetReviewsByProductQuery request, CancellationToken cancellationToken)
    {
        var productId = request.ProductId;

        try
        {
            logger.LogInformation("Retrieving reviews for product {ProductId}", productId);

            var reviews = await context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

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
}