using Application.Features.Auth.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand(int userId, ChangePasswordDto dto) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public ChangePasswordDto Dto { get; } = dto;
}


