using Domain.Entities.CategoryEntity;

namespace Domain.Entities.ProductEntity;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Sku { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int SellerProfileId { get; set; }
    public SellerProfile SellerProfile { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public ICollection<ProductImage> Images { get; set; }
    public ICollection<ProductVariant> Variants { get; set; }
}