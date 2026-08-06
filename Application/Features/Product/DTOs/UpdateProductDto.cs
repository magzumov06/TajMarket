namespace Application.Features.Product.DTOs;

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    bool IsActive
);