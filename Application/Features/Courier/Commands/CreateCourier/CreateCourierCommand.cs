using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Commands.CreateCourier;

public class CreateCourierCommand(CreateCourierDto dto) : IRequest<Response<CourierDto>>
{
    public CreateCourierDto Dto { get; } = dto;
}