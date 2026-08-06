namespace Application.Features.Product.DTOs;

public record ProductVariantDto(int Id,
    string Name,
    string Value,
    decimal? ExtraPrice,
    int StockQuantity);
