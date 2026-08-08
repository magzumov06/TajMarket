using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Review.Commands.DeleteReview;

public class DeleteReviewCommandHandler(
    IApplicationDbContext context,
    ILogger<DeleteReviewCommandHandler> logger)
    : IRequestHandler<DeleteReviewCommand, Response<string>>
{
    public async Task<Response<string>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var (userId, reviewId) = (request.UserId, request.ReviewId);

        try
        {
            logger.LogInformation("Deleting review {ReviewId} by user {UserId}", reviewId, userId);

            var review = await context.Reviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

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

            var sellerProfileId = review.Product.SellerProfileId;

            context.Reviews.Remove(review);
            await context.SaveChangesAsync(cancellationToken);

            await RecalculateSellerRatingHelper.RecalculateAsync(context, sellerProfileId, logger, cancellationToken);

            logger.LogInformation("Review deleted successfully {ReviewId}", reviewId);

            return new Response<string>(HttpStatusCode.OK, "Шарҳ нест шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting review {ReviewId}", reviewId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}