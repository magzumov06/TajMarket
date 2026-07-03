namespace Domain.DTOs.ProductDto;

public record ProductVariantDto(int Id,
    string Name,
    string Value,
    decimal? ExtraPrice,
    int StockQuantity);
