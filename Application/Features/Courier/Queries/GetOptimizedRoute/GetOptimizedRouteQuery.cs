using Application.Features.Courier.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetOptimizedRoute;

public class GetOptimizedRouteQuery(int courierUserId) : IRequest<Response<OptimizedRouteDto>>
{
    public int CourierUserId { get; } = courierUserId;
}