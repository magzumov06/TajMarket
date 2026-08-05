
using Domain.DTOs.ReviewDtos;
using Domain.DTOs.SellerDto;

namespace Domain.DTOs.ProductDto;

public record ProductDetailDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    List<string> ImageUrls,
    List<ProductVariantDto> Variants,
    CategoryDto Category,
    SellerProfileDto Seller,
    decimal AverageRating,
    List<ReviewDto> Reviews
);

