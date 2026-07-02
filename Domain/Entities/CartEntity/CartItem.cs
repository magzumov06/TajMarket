using Domain.Entities.ProductEntity;

namespace Domain.Entities.CartEntity;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart Cart { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; }
}