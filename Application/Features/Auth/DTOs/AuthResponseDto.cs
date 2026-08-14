namespace Application.Features.Auth.DTOs;

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    int UserId,
    string FullName,
    string Email,
    List<string> Roles
);