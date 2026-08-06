namespace Application.Features.Review.DTOs;

public record ReviewDto(
    int Id,
    string UserFullName,
    string? UserAvatarUrl,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);