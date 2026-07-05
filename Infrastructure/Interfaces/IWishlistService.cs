using Domain.DTOs.WishlistDto;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public interface IWishlistService
{
    Task<Responce<List<WishlistItemDto>>> GetAllAsync(int userId);
    Task<Responce<string>> AddAsync(int userId, int productId);
    Task<Responce<string>> RemoveAsync(int userId, int productId);
}