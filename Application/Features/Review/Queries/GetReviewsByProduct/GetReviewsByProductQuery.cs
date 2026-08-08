using Application.Features.Review.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Review.Queries.GetReviewsByProduct;

public class GetReviewsByProductQuery(int productId) : IRequest<Response<List<ReviewDto>>>
{
    public int ProductId { get; } = productId;
}