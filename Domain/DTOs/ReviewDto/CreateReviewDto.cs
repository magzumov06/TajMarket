namespace Domain.DTOs.ReviewDto;

public record CreateReviewDto(int ProductId, int Rating, string? Comment);
