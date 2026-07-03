using Domain.Enums;

namespace Domain.DTOs.OrderDto;

public record OrderListDto(
    int Id,
    string OrderNumber,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemsCount
);