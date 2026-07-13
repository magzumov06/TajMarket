using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.AuthDto;

public class RegisterDto
{
    public required string FullName {get; set;}
    public required string Email{get; set;}
    public required string Password{get; set;}

    [Phone]
    [StringLength(13, MinimumLength = 9, ErrorMessage = "Phone length must be between 9 and 13")]
    public required string PhoneNumber{get; set;}
}