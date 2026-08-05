using Application.Features.Wishlist.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Wishlist.Queries.GetWishlist;

public class GetWishlistQuery(int userId) : IRequest<Response<List<WishlistItemDto>>>
{
    public int UserId { get; } = userId;
}