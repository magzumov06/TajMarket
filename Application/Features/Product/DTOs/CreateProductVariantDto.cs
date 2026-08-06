namespace Application.Features.Product.DTOs;

public record CreateProductVariantDto(
    string Name,
    string Value,
    decimal? ExtraPrice,
    int StockQuantity
    );
