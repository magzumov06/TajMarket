using Application.Features.Courier.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Commands.UpdateCourier;

public class UpdateCourierCommand(int courierId, UpdateCourierDto dto) : IRequest<Response<CourierDto>>
{
    public int CourierId { get; } = courierId;
    public UpdateCourierDto Dto { get; } = dto;
}