namespace Application.Features.Order.DTOs;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);