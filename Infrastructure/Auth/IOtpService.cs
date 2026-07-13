using Domain.DTOs.AuthDto;
using Domain.Responses;

namespace Infrastructure.Auth;

public interface IOtpService
{
    Task<Response<string>> VerifyOtpAsync(VerifyOtpDto dto);
    Task<Response<string>> ResendOtpAsync(string email);
}