using Application.Features.Dashboard.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Dashboard.Queries.GetTopProducts;

public class GetTopProductsQuery(int count = 10) : IRequest<Response<List<TopProductDto>>>
{
    public int Count { get; } = count;
}