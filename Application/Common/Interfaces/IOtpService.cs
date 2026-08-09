using Application.Features.Auth.DTOs;
using Domain.Responses;

namespace Application.Common.Interfaces;

public interface IOtpService
{
    Task<Response<string>> VerifyOtpAsync(VerifyOtpDto dto);
    Task<Response<string>> ResendOtpAsync(string email);
}