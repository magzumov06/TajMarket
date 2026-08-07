using Application.Features.Product.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Product.Queries.GetProductList;

public class GetProductListQuery(ProductFilter filter) : IRequest<PaginationResponse<List<ProductListDto>>>
{
    public ProductFilter Filter { get; } = filter;
}