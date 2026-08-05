using System.Net;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Features.Coupon.Queries.ValidateCoupon;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Order;

internal class PricingResult
{
    public required Domain.Entities.CartEntity.Cart Cart { get; init; }
    public required Domain.Entities.AddressEntity.Address Address { get; init; }   // ← номи пурра
    public required decimal SubTotal { get; init; }
    public required decimal DiscountAmount { get; init; }
    public int? AppliedCouponId { get; init; }
    public required decimal ShippingCost { get; init; }
    public decimal TotalAmount => SubTotal - DiscountAmount + ShippingCost;
}

internal static class OrderPricingHelper
{
    public static async Task<(PricingResult? Result, HttpStatusCode ErrorStatus, string? ErrorMessage)> BuildPricingAsync(
        IApplicationDbContext context,
        IMediator mediator,
        ShippingSetting shippingSettings,
        ILogger logger,
        int userId,
        int shippingAddressId,
        string? couponCode,
        CancellationToken cancellationToken)
    {
        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == shippingAddressId && a.UserId == userId, cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Shipping address not found {ShippingAddressId} for user {UserId}", shippingAddressId, userId);
            return (null, HttpStatusCode.BadRequest, "Address not found");
        }

        var cart = await context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null || cart.Items.Count == 0)
        {
            logger.LogWarning("Cart is empty for user {UserId}", userId);
            return (null, HttpStatusCode.BadRequest, "Cart is empty");
        }

        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
            {
                logger.LogWarning("Product {ProductId} is inactive", item.ProductId);
                return (null, HttpStatusCode.BadRequest, $"Маҳсулоти '{item.Product.Name}' дигар дастрас нест");
            }

            var availableStock = item.ProductVariant?.StockQuantity ?? item.Product.StockQuantity;

            if (item.Quantity > availableStock)
            {
                logger.LogWarning("Not enough stock for product {ProductId}", item.ProductId);
                return (null, HttpStatusCode.BadRequest,
                    $"Барои '{item.Product.Name}' танҳо {availableStock} дона дар анбор мондааст");
            }
        }

        var subTotal = cart.Items.Sum(i => UnitPrice(i) * i.Quantity);

        decimal discountAmount = 0;
        int? appliedCouponId = null;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var validation = await mediator.Send(new ValidateCouponQuery(couponCode, subTotal), cancellationToken);

            if (!validation.Success)
            {
                logger.LogWarning("Invalid coupon {CouponCode}", couponCode);
                return (null, HttpStatusCode.BadRequest, validation.Message ?? "Купон нодуруст аст");
            }

            discountAmount = Math.Min(validation.Data!.DiscountAmount, subTotal);
            appliedCouponId = validation.Data.CouponId;
        }

        var shippingCost = CalculateShippingCost(address, shippingSettings, logger);

        var result = new PricingResult
        {
            Cart = cart,
            Address = address,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            AppliedCouponId = appliedCouponId,
            ShippingCost = shippingCost
        };

        return (result, HttpStatusCode.OK, null);
    }

    private static decimal CalculateShippingCost(
        Domain.Entities.AddressEntity.Address address,   // ← номи пурра
        ShippingSetting settings,
        ILogger logger)
    {
        var isSameCity = string.Equals(
            address.City?.Trim(),
            settings.WarehouseCity?.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (isSameCity)
        {
            logger.LogInformation("Shipping cost: same city ({City}), flat rate {Rate}", address.City, settings.SameCityFlatRate);
            return settings.SameCityFlatRate;
        }

        if (address.Latitude.HasValue && address.Longitude.HasValue)
        {
            var distanceKm = DistanceKm(
                settings.WarehouseLatitude, settings.WarehouseLongitude,
                address.Latitude.Value, address.Longitude.Value);

            var calculated = Math.Round((decimal)distanceKm * settings.PricePerKm, 2);
            var finalCost = Math.Max(calculated, settings.MinOtherCityRate);

            logger.LogInformation("Shipping cost: other city ({City}), distance {Distance} km, cost {Cost}", address.City, distanceKm, finalCost);
            return finalCost;
        }

        logger.LogInformation("Shipping cost: other city ({City}) without coordinates, fallback rate {Rate}", address.City, settings.MinOtherCityRate);
        return settings.MinOtherCityRate;
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    public static decimal UnitPrice(Domain.Entities.CartEntity.CartItem item) =>
        (item.Product.DiscountPrice ?? item.Product.Price)
        + (item.ProductVariant?.ExtraPrice ?? 0);

    public static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
}