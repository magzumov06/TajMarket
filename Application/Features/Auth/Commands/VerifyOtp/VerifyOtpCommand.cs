using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommand(VerifyOtpDto dto) : IRequest<Response<string>>
{
    public VerifyOtpDto Dto { get; } = dto;
}