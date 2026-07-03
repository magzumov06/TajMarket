namespace Domain.DTOs.CartDto;
public record AddToCartDto
{
    public int ProductId { get; init; }
    public int? ProductVariantId { get; init; }
    public int Quantity { get; init; } = 1;
}