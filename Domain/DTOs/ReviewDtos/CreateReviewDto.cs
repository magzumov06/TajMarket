namespace Domain.DTOs.ReviewDtos;

public record CreateReviewDto(
    int ProductId, 
    int Rating,
    string? Comment
    );
