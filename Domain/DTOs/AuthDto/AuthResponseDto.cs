namespace Domain.DTOs.AuthDto;

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    int UserId,
    string FullName,
    string Email,
    List<string> Roles
);