using Domain.DTOs.ReviewDtos;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IReviewService
{
    Task<Response<string>> CreateAsync(int userId, CreateReviewDto dto);
    Task<Response<string>> DeleteAsync(int userId, int reviewId);
    Task<Response<List<ReviewDto>>> GetByProductAsync(int productId);
}