using Application.Features.Product.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Product.Queries.GetProductDetail;

public class GetProductDetailQuery(int productId) : IRequest<Response<ProductDetailDto>>
{
    public int ProductId { get; } = productId;
}