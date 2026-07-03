namespace Domain.DTOs.SellerDto;

public record SellerProfileDto(
    int Id,
    string StoreName,
    string? StoreDescription,
    string? StoreLogoUrl,
    bool IsVerified,
    decimal Rating,
    int TotalProducts
);