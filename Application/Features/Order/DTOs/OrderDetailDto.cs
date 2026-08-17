using Application.Features.Address.DTOs;
using Application.Features.Payment.DTOs;
using Domain.DTOs.PaymentDtos;
using Domain.Enums;

namespace Application.Features.Order.DTOs;

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
    DateTime? CustomerConfirmedAt ,
    string? DeliveryConfirmationCode  
);