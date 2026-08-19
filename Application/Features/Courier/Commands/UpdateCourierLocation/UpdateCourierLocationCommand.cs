using Application.Features.Courier.DTOs;
using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Commands.UpdateCourierLocation;

public class UpdateCourierLocationCommand(int userId, UpdateCourierLocationDto dto) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public UpdateCourierLocationDto Dto { get; } = dto;
}