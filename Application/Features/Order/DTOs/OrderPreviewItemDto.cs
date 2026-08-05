namespace Application.Features.Order.DTOs;

public record OrderPreviewItemDto(
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);