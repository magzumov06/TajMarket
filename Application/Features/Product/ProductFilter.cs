using Domain.Filters;

namespace Application.Features.Product;

public class ProductFilter : BaseFilter
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public string? SortBy { get; set; }
}