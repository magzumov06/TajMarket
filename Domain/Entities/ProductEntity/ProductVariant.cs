namespace Domain.Entities.ProductEntity;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public decimal? ExtraPrice { get; set; }
    public int StockQuantity { get; set; }
}