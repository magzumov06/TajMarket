using Domain.DTOs.ReviewDtos;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public interface IReviewService
{
    Task<Responce<List<ReviewDto>>> GetByProductAsync(int productId);
    Task<Responce<string>> CreateAsync(int userId, CreateReviewDto dto);
    Task<Responce<string>> DeleteAsync(int userId, int reviewId);
}