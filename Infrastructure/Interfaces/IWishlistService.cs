using Domain.DTOs.WishlistDto;
using Domain.Resposes;

namespace Infrastructure.Interfaces;

public interface IWishlistService
{
    Task<Response<List<WishlistItemDto>>> GetAllAsync(int userId);
    Task<Response<string>> AddAsync(int userId, int productId);
    Task<Response<string>> RemoveAsync(int userId, int productId);
}