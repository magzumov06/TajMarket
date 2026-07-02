using Domain.Entities.ProductEntity;

namespace Domain.Entities.CategoryEntity;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? IconUrl { get; set; }
    public string? IconPublicId { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; }
    public ICollection<Product> Products { get; set; }
}