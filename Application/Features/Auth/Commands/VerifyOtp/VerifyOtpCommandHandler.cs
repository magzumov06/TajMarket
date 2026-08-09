using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler(IOtpService otpService) : IRequestHandler<VerifyOtpCommand, Response<string>>
{
    public async Task<Response<string>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken) =>
        await otpService.VerifyOtpAsync(request.Dto);
}