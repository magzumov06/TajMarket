namespace Application.Features.Wishlist.DTOs;

public record WishlistItemDto(
    int Id,
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal Price,
    DateTime AddedAt
);