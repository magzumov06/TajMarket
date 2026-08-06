using Application.Features.Product.Dtos;
using Application.Features.Product.DTOs;
using Domain.DTOs.ProductDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Product.Commands.CreateProduct;

public class CreateProductCommand(int sellerUserId, CreateProductDto dto) : IRequest<Response<string>>
{
    public int SellerUserId { get; } = sellerUserId;
    public CreateProductDto Dto { get; } = dto;
}