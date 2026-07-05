using Domain.DTOs.OrderDto;
using Domain.DTOs.PaymentDtos;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public interface IPaymentService
{
    Task<Responce<PaymentDto>> GetByOrderIdAsync(int userId, int orderId);
    Task<Responce<OrderDetailDto>> ProcessAsync(int userId, ProcessPaymentDto dto);
}