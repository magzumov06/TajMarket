using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetCourierById;

public class GetCourierByIdQuery(int courierId) : IRequest<Response<CourierDto>>
{
    public int CourierId { get; } = courierId;
}