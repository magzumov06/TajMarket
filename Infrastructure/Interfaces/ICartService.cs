using Domain.DTOs.CartDto;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public interface ICartService
{
    Task<Responce<string>> AddToCartAsync(int userId, AddToCartDto dto);
    Task<Responce<string>> UpdateCartItemAsync(int userId,int cartItemId, UpdateCartItemDto dto);
    Task<Responce<string>> RemoveFromCartAsync(int userId, int cartItemId);
    Task<Responce<List<CartItemDto>>> GetCartItemsAsync(int userId);
    Task<Responce<string>> ClearCartAsync(int userId);
}