using Domain.DTOs.OrderDto;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public interface IOrderService
{
    Task<Responce<string>> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<Responce<string>> CancelOrderAsync(int userId, int orderId);
    Task<Responce<List<OrderListDto>>> GetOrderListAsync(int userId);
    Task<Responce<OrderDetailDto>> GetOrderDetailAsync(int orderId, int userId);
    Task<Responce<List<OrderListDto>>> GetBySellerIdAsync(int sellerUserId);
    Task<Responce<string>> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto);
}