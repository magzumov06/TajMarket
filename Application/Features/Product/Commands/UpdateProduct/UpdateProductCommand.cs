using Application.Features.Product.DTOs;
using Domain.DTOs.ProductDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Product.Commands.UpdateProduct;

public class UpdateProductCommand(int sellerUserId, int productId, UpdateProductDto dto) : IRequest<Response<string>>
{
    public int SellerUserId { get; } = sellerUserId;
    public int ProductId { get; } = productId;
    public UpdateProductDto Dto { get; } = dto;
}