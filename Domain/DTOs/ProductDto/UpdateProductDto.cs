namespace Domain.DTOs.ProductDto;

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    bool IsActive
);