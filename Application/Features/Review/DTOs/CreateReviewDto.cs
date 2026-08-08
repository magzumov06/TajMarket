namespace Application.Features.Review.DTOs;

public record CreateReviewDto(
    int ProductId, 
    int Rating,
    string? Comment
    );
