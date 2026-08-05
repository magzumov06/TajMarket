using Application.Features.Wishlist.DTOs;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IWishlistService
{
    Task<Response<string>> AddAsync(int userId, int productId);
    Task<Response<string>> RemoveAsync(int userId, int productId);
    Task<Response<List<WishlistItemDto>>> GetAllAsync(int userId);
}