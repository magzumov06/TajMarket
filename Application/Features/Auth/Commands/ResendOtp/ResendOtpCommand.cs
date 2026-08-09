using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.ResendOtp;

public class ResendOtpCommand(string email) : IRequest<Response<string>>
{
    public string Email { get; } = email;
}