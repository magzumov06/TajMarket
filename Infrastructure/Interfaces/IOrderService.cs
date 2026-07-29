using Domain.DTOs.OrderDto;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IOrderService
{
    Task<Response<string>> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<Response<string>> CancelOrderAsync(int userId, int orderId);
    Task<PaginationResponse<List<OrderListDto>>> GetOrderListAsync(int userId, OrderFilter filter);
    Task<Response<OrderDetailDto>> GetOrderDetailAsync(int orderId, int userId);
    Task<PaginationResponse<List<OrderListDto>>> GetBySellerIdAsync(int sellerUserId, OrderFilter filter);
    Task<Response<OrderDetailDto>> UpdateStatusAsync(int sellerUserId, int orderId, UpdateOrderStatusDto dto);

    Task<Response<OrderPreviewDto>> CalculateTotalPriceAsync(int userId, CalculateTotalPriceDto dto);   
    Task<Response<OrderDetailDto>> CompleteOrderAsync(int userId, int orderId);                         
}