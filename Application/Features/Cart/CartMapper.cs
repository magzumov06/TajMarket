using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cart;

internal static class CartMapper
{
    public static async Task<CartDto> MapAsync(IApplicationDbContext context, int cartId, CancellationToken cancellationToken)
    {
        var items = await context.CartItems
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Include(ci => ci.ProductVariant)
            .Where(ci => ci.CartId == cartId)
            .ToListAsync(cancellationToken);

        var itemDtos = items
            .Select(ci =>
            {
                var unitPrice =
                    (ci.Product.DiscountPrice ?? ci.Product.Price)
                    + (ci.ProductVariant?.ExtraPrice ?? 0);

                return new CartItemDto(
                    ci.Id,
                    ci.ProductId,
                    ci.Product.Name,
                    ci.Product.Images?.FirstOrDefault()?.Url,
                    unitPrice,
                    ci.Quantity,
                    unitPrice * ci.Quantity,
                    ci.ProductVariantId,
                    ci.ProductVariant != null
                        ? $"{ci.ProductVariant.Name}: {ci.ProductVariant.Value}"
                        : null
                );
            })
            .ToList();

        return new CartDto(cartId, itemDtos, itemDtos.Sum(i => i.TotalPrice));
    }
}