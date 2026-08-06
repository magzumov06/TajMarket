using Application.Features.Review.DTOs;
using Domain.DTOs.ReviewDtos;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IReviewService
{
    Task<Response<ReviewDto>> CreateAsync(int userId, CreateReviewDto dto);
    Task<Response<string>> DeleteAsync(int userId, int reviewId);
    Task<Response<List<ReviewDto>>> GetByProductAsync(int productId);
}