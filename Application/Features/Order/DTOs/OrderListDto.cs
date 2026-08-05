using Domain.Enums;

namespace Application.Features.Order.DTOs;

public record OrderListDto(
    int Id,
    string OrderNumber,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemsCount
);