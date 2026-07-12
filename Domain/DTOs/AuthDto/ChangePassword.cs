using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.AuthDto;

public class ChangePassword
{
    [DataType(DataType.Password)] public required string OldPassword { get; set; }
    [DataType(DataType.Password)] public required string Password { get; set; }
    [Compare("Password")] public required string ConfirmPassword { get; set; }
}