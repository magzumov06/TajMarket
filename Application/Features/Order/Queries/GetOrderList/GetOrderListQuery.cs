using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderList;

public class GetOrderListQuery(int userId, OrderFilter filter) : IRequest<PaginationResponse<List<OrderListDto>>>
{
    public int UserId { get; } = userId;
    public OrderFilter Filter { get; } = filter;
}