using Domain.Responses;
using MediatR;

namespace Application.Features.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommand(int userId, int productId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int ProductId { get; } = productId;
}