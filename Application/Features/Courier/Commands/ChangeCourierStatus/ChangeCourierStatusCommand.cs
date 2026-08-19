using Application.Features.Courier.DTOs;
using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Commands.ChangeCourierStatus;

public class ChangeCourierStatusCommand(int userId, UpdateCourierStatusDto dto) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public UpdateCourierStatusDto Dto { get; } = dto;
}