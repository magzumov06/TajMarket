using Application.Features.Courier.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetMyOrders;

public class GetMyOrdersQuery(int userId) : IRequest<Response<List<CourierOrderDto>>>
{
    public int UserId { get; } = userId;
}