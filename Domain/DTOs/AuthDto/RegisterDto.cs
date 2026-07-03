namespace Domain.DTOs.AuthDto;

public record RegisterDto(string FullName, string Email, string Password, string PhoneNumber);
