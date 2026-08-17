using Application.Features.Dashboard.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Dashboard.Queries.GetTopSellers;

public class GetTopSellersQuery(int count = 10) : IRequest<Response<List<TopSellerDto>>>
{
    public int Count { get; } = count;
}