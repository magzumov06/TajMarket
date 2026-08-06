using Application.Features.Cart.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Cart.Commands.AddToCart;

public class AddToCartCommand(int userId, AddToCartDto dto) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public AddToCartDto Dto { get; } = dto;
}