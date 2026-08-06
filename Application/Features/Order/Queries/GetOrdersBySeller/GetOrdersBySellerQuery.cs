using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.GetOrdersBySeller;

public class GetOrdersBySellerQuery(int sellerUserId, OrderFilter filter) : IRequest<PaginationResponse<List<OrderListDto>>>
{
    public int SellerUserId { get; } = sellerUserId;
    public OrderFilter Filter { get; } = filter;
}