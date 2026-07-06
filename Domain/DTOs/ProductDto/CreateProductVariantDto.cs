namespace Domain.DTOs.ProductDto;

public record CreateProductVariantDto(
    string Name,
    string Value,
    decimal? ExtraPrice,
    int StockQuantity
    );
