using Application.Features.Cart.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemCommand(int userId, int cartItemId, UpdateCartItemDto dto) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int CartItemId { get; } = cartItemId;
    public UpdateCartItemDto Dto { get; } = dto;
}