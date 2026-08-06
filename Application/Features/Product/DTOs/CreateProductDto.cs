using Domain.DTOs.ProductDto;

namespace Application.Features.Product.DTOs;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    int CategoryId,
    List<CreateProductVariantDto>? Variants
);
