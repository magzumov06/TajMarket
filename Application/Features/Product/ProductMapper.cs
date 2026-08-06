using Application.Features.Product.DTOs;

namespace Application.Features.Product;

internal static class ProductMapper
{
    public static ProductListDto ToListDto(Domain.Entities.ProductEntity.Product p) => new(
        p.Id,
        p.Name,
        p.Slug,
        p.Price,
        p.DiscountPrice,

        p.Images.FirstOrDefault(i => i.IsMain)?.Url
        ?? p.Images.FirstOrDefault()?.Url,

        p.Reviews.Count != 0
            ? decimal.Round(p.Reviews.Average(r => (decimal)r.Rating), 1)
            : 0,

        p.Reviews.Count,

        p.SellerProfile.StoreName
    );
}