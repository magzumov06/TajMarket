namespace Domain.DTOs.OrderDto;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);