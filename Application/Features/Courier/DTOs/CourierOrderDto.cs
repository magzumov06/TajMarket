using Domain.Enums;

namespace Domain.DTOs.CourierDto;

public record CourierOrderDto(
    int Id,
    string OrderNumber,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    string ShippingCity,
    string ShippingStreet,
    string? CustomerFullName,
    string? CustomerPhone
);