using Application.Features.Auth.Dtos;
using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand(RefreshTokenDto dto) : IRequest<Response<AuthResponseDto>>
{
    public RefreshTokenDto Dto { get; } = dto;
}