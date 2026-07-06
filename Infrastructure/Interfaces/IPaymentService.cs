using Domain.DTOs.OrderDto;
using Domain.DTOs.PaymentDtos;
using Domain.Resposes;

namespace Infrastructure.Interfaces;

public interface IPaymentService
{
    Task<Response<PaymentDto>> GetByOrderIdAsync(int userId, int orderId);
    Task<Response<OrderDetailDto>> ProcessAsync(int userId, ProcessPaymentDto dto);
}