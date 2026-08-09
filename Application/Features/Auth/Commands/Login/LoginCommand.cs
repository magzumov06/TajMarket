using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommand(LoginDto dto) : IRequest<Response<AuthResponseDto>>
{
    public LoginDto Dto { get; } = dto;
}