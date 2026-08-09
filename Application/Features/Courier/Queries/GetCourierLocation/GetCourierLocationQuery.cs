using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetCourierLocation;

public class GetCourierLocationQuery(int courierId) : IRequest<Response<CourierLocationDto>>
{
    public int CourierId { get; } = courierId;
}