using Application.Features.Category.DTOs;
using Application.Features.Review.DTOs;
using Application.Features.Seller.DTOs;
using Domain.DTOs.ReviewDtos;
using Domain.DTOs.SellerDto;

namespace Application.Features.Product.DTOs;

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

