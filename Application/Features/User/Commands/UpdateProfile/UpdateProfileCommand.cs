using Application.Features.User.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.User.Commands.UpdateProfile;

public class UpdateProfileCommand(int userId, UpdateUserDto dto) : IRequest<Response<UserProfileDto>>
{
    public int UserId { get; } = userId;
    public UpdateUserDto Dto { get; } = dto;
}