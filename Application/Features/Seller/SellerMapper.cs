using Application.Features.Seller.DTOs;
using Domain.Entities.UserEntity;

namespace Application.Features.Seller;

internal static class SellerMapper
{
    public static SellerProfileDto ToDto(SellerProfile profile, int totalProducts) => new(
        profile.Id,
        profile.StoreName,
        profile.StoreDescription,
        profile.StoreLogoUrl,
        profile.IsVerified,
        profile.Rating,
        totalProducts
    );
}