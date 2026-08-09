namespace Application.Features.User.DTOs;

public record UserProfileDto(
    int Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    List<string> Roles
);