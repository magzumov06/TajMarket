using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.AuthDto;

public record RegisterDto(
    string FullName,
    string Email,
    string Password,
    [Phone]
    [StringLength(13 , MinimumLength = 9 , ErrorMessage = "Phone length must be between 9 and 13")]
    string PhoneNumber);
