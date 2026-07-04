namespace Domain.DTOs.ReviewDtos;

public record ReviewDto(
    int Id,
    string UserFullName,
    string? UserAvatarUrl,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);