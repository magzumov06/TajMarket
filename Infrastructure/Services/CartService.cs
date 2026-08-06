using System.Net;
using Application.Features.Cart.DTOs;
using Domain.Entities.CartEntity;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class CartService(
    DataContext context,
    ILogger<CartService> logger) : ICartService
{
    public async Task<Response<string>> AddToCartAsync(int userId, AddToCartDto dto)
    {
        logger.LogInformation("AddToCartAsync started for user {UserId} product {ProductId} variant {ProductVariantId} quantity {Quantity}",
            userId, dto.ProductId, dto.ProductVariantId, dto.Quantity);

        try
        {
            var product = await context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);

            if (product == null)
                return new Response<string>(HttpStatusCode.NotFound, "Product not found");

            if (dto.Quantity <= 0)
                return new Response<string>(HttpStatusCode.BadRequest, "Quantity must be greater than zero");

            var availableStock = product.StockQuantity;

            if (dto.ProductVariantId.HasValue)
            {
                var variant = product.Variants
                    .FirstOrDefault(v => v.Id == dto.ProductVariantId);

                if (variant == null)
                    return new Response<string>(HttpStatusCode.NotFound, "Variant not found");

                availableStock = variant.StockQuantity;
            }

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await context.CartItems.FirstOrDefaultAsync(
                ci =>
                    ci.CartId == cart.Id &&
                    ci.ProductId == dto.ProductId &&
                    ci.ProductVariantId == dto.ProductVariantId);

            var requestedTotalQuantity =
                (existingItem?.Quantity ?? 0) + dto.Quantity;

            if (requestedTotalQuantity > availableStock)
                return new Response<string>(HttpStatusCode.BadRequest, $"Танҳо {availableStock} дона дар анбор мондааст");


            if (existingItem != null)
            {
                existingItem.Quantity = requestedTotalQuantity;
            }
            else
            {
                context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity,
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();


            logger.LogInformation("Product {ProductId} added to cart for user {UserId}", dto.ProductId, userId);


            return new Response<string>(HttpStatusCode.OK, "Ба сабад илова шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "AddToCartAsync failed for user {UserId} product {ProductId}", userId, dto.ProductId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<string>> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
    {
        logger.LogInformation("UpdateCartItemAsync started for user {UserId} cartItem {CartItemId} quantity {Quantity}", userId, cartItemId, dto.Quantity);

        try
        {
            var item = await context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .Include(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(
                    ci =>
                        ci.Id == cartItemId &&
                        ci.Cart.UserId == userId);


            if (item == null)
            {
                logger.LogWarning("Cart item not found {CartItemId} for user {UserId}", cartItemId, userId);

                return new Response<string>(HttpStatusCode.NotFound, "Ин ашё дар сабад ёфт нашуд");
            }


            if (dto.Quantity <= 0)
                return new Response<string>(HttpStatusCode.BadRequest, "Миқдор бояд аз сифр зиёд бошад");


            var availableStock =
                item.ProductVariant?.StockQuantity ??
                item.Product.StockQuantity;


            if (dto.Quantity > availableStock)
                return new Response<string>(HttpStatusCode.BadRequest, $"Танҳо {availableStock} дона дар анбор мондааст");
            
            item.Quantity = dto.Quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            
            logger.LogInformation("Cart item updated {CartItemId} for user {UserId}", cartItemId, userId);


            return new Response<string>(HttpStatusCode.OK, "Cart updated");
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateCartItemAsync failed for cart item {CartItemId} user {UserId}", cartItemId, userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> RemoveFromCartAsync(int userId,int cartItemId)
    {
        try
        {
            logger.LogInformation("Removing cart item {CartItemId} for user {UserId}", cartItemId, userId);
            
            var item = await context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(
                    ci =>
                        ci.Id == cartItemId &&
                        ci.Cart.UserId == userId);


            if (item == null)
            {
                logger.LogWarning("Cart item not found {CartItemId} for user {UserId}", cartItemId, userId);

                return new Response<string>(HttpStatusCode.NotFound, "Ин ашё дар сабад ёфт нашуд");
            }

            var cartId = item.CartId;
            
            context.CartItems.Remove(item);

            await context.SaveChangesAsync();

            logger.LogInformation("Cart item {CartItemId} removed from cart {CartId}", cartItemId, cartId);


            return new Response<string>(HttpStatusCode.OK, "Item removed");
        }
        catch (Exception e)
        {
            logger.LogError(e, "RemoveFromCartAsync failed for cart item {CartItemId} and user {UserId}", cartItemId, userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<CartDto>> GetCartItemsAsync(int userId)
    {
        try
        {
            logger.LogInformation("Retrieving cart items for user {UserId}",userId);
            
            var cart = await GetOrCreateCartAsync(userId);

            var cartDto = await MapAsync(cart.Id);

            logger.LogInformation("Retrieved {ItemCount} cart items for user {UserId}", cartDto.Items.Count, userId);
            
            return new Response<CartDto>(cartDto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetCartItemsAsync failed for user {UserId}", userId);

            return new Response<CartDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<string>> ClearCartAsync(int userId)
    {
        try
        {
            logger.LogInformation("Clearing cart for user {UserId}", userId);


            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);


            if (cart?.Items != null && cart.Items.Count != 0)
            {
                context.CartItems.RemoveRange(cart.Items);

                await context.SaveChangesAsync();
            }


            logger.LogInformation("Cart cleared for user {UserId}",userId);
            
            return new Response<string>(HttpStatusCode.OK, "Cart cleared");
        }
        catch (Exception e)
        {
            logger.LogError(e, "ClearCartAsync failed for user {UserId}", userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    private async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId);


        if (cart != null)
            return cart;


        cart = new Cart
        {
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        };


        context.Carts.Add(cart);

        await context.SaveChangesAsync();


        logger.LogInformation(
            "New cart created for user {UserId}",
            userId);


        return cart;
    }


    private async Task<CartDto> MapAsync(int cartId)
    {
        var items = await context.CartItems
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Include(ci => ci.ProductVariant)
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();


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


        return new CartDto(
            cartId,
            itemDtos,
            itemDtos.Sum(i => i.TotalPrice));
    }
}