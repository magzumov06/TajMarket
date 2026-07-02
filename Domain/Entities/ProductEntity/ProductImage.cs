namespace Domain.Entities.ProductEntity;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public string Url { get; set; }
    public string PublicId { get; set; }    
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}