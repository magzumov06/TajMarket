using Domain.DTOs.OrderDto;
using Domain.DTOs.PaymentDtos;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IPaymentService
{
    Task<Response<OrderDetailDto>> ProcessAsync(int userId, ProcessPaymentDto dto);
    Task<Response<PaymentDto>> GetByOrderIdAsync(int userId, int orderId);
}