using Application.Features.Review.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Review.Commands.CreateReview;

public class CreateReviewCommand(int userId, CreateReviewDto dto) : IRequest<Response<ReviewDto>>
{
    public int UserId { get; } = userId;
    public CreateReviewDto Dto { get; } = dto;
}