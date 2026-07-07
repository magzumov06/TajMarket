using Domain.DTOs.OrderDto;
using Domain.Entities.OrderEntity;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IOrderService
{
    Task<Response<string>> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<Response<string>> CancelOrderAsync(int userId, int orderId);
    Task<Response<List<OrderListDto>>> GetOrderListAsync(int userId);
    Task<Response<OrderDetailDto>> GetOrderDetailAsync(int orderId, int userId);
    Task<Response<List<OrderListDto>>> GetBySellerIdAsync(int sellerUserId);
    Task<Response<OrderDetailDto>> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto);
}