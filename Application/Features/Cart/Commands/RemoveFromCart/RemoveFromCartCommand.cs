using Domain.Responses;
using MediatR;

namespace Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartCommand(int userId, int cartItemId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int CartItemId { get; } = cartItemId;
}