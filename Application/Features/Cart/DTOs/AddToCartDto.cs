namespace Application.Features.Cart.DTOs;
public record AddToCartDto
{
    public int ProductId { get; init; }
    public int? ProductVariantId { get; init; }
    public int Quantity { get; init; } = 1;
}