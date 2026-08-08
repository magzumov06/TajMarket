using Domain.Responses;
using MediatR;

namespace Application.Features.Review.Commands.DeleteReview;

public class DeleteReviewCommand(int userId, int reviewId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int ReviewId { get; } = reviewId;
}