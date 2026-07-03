namespace Domain.DTOs.ProductDto;

public record ProductListDto(
    int Id,
    string Name,
    string Slug,
    decimal Price,
    decimal? DiscountPrice,
    string? MainImageUrl,
    decimal AverageRating,
    int ReviewCount,
    string SellerStoreName
);