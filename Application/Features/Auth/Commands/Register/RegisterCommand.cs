using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommand(RegisterDto dto) : IRequest<Response<AuthResponseDto>>
{
    public RegisterDto Dto { get; } = dto;
}