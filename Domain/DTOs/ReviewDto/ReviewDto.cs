namespace Domain.DTOs.ReviewDto;

public record ReviewDto(
    int Id,
    string UserFullName,
    string? UserAvatarUrl,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);