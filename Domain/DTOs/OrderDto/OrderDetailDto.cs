using Domain.DTOs.AddressDtos;
using Domain.DTOs.PaymentDtos;
using Domain.Enums;
namespace Domain.DTOs.OrderDto;

public record OrderDetailDto(
    int Id,
    string OrderNumber,
    DateTime OrderDate,
    OrderStatus Status,
    decimal SubTotal,
    decimal ShippingCost,
    decimal DiscountAmount,
    decimal TotalAmount, 
    AddressDto ShippingAddress,
    List<OrderItemDto> Items,
    PaymentDto? Payment,
    CourierInfoDto? Courier,
    DateTime? CustomerConfirmedAt 
);