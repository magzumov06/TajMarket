namespace Application.Features.Cart.DTOs;

public record CartItemDto(
    int Id,
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    int? ProductVariantId,
    string? VariantInfo
);