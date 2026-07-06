using Domain.DTOs.ReviewDtos;
using Domain.Resposes;

namespace Infrastructure.Interfaces;

public interface IReviewService
{
    Task<Response<List<ReviewDto>>> GetByProductAsync(int productId);
    Task<Response<string>> CreateAsync(int userId, CreateReviewDto dto);
    Task<Response<string>> DeleteAsync(int userId, int reviewId);
}