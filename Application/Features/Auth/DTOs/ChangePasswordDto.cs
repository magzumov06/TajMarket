using System.ComponentModel.DataAnnotations;

namespace Application.Features.Auth.DTOs;

public class ChangePasswordDto
{
    [DataType(DataType.Password)] public required string OldPassword { get; set; }
    [DataType(DataType.Password)] public required string Password { get; set; }
    [Compare("Password")] public required string ConfirmPassword { get; set; }
}