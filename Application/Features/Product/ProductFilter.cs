using Domain.Filters;

namespace Application.Features.Product;

public class ProductFilter : BaseFilter
{
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? MinRating { get; init; }
    public string? SortBy { get; init; }
}