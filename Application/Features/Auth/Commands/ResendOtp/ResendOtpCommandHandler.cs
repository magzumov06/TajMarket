using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.ResendOtp;

public class ResendOtpCommandHandler(IOtpService otpService) : IRequestHandler<ResendOtpCommand, Response<string>>
{
    public async Task<Response<string>> Handle(ResendOtpCommand request, CancellationToken cancellationToken) =>
        await otpService.ResendOtpAsync(request.Email);
}