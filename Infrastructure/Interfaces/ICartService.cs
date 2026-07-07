using Domain.DTOs.CartDto;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface ICartService
{
    Task<Response<string>> AddToCartAsync(int userId, AddToCartDto dto);
    Task<Response<string>> UpdateCartItemAsync(int userId,int cartItemId, UpdateCartItemDto dto);
    Task<Response<string>> RemoveFromCartAsync(int userId, int cartItemId);
    Task<Response<CartDto>> GetCartItemsAsync(int userId);
    Task<Response<string>> ClearCartAsync(int userId);
}