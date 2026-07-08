using System.Net;
using Domain.DTOs.CartDto;
using Domain.Entities.CartEntity;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class CartService(DataContext context) : ICartService
{
    public async Task<Response<string>> AddToCartAsync(int userId, AddToCartDto dto)
    {
        Log.ForContext<CartService>().Information("AddToCartAsync started for user {UserId} product {ProductId} variant {ProductVariantId} quantity {Quantity}", userId, dto.ProductId, dto.ProductVariantId, dto.Quantity);
        try
        {
            var product = await context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);
            
            if (product == null)
                return new Response<string>(HttpStatusCode.NotFound, "Product not found");
            
            if(dto.Quantity <= 0)
                return new Response<string>(HttpStatusCode.BadRequest, "Quantity must be greater than zero");

            var availableStock = product.StockQuantity;
            if (dto.ProductVariantId.HasValue)
            {
                var variant =product.Variants.FirstOrDefault(v => v.Id == dto.ProductVariantId);
                if (variant == null)
                    return new Response<string>(HttpStatusCode.NotFound, "Variant not found");
                availableStock =  variant.StockQuantity;
            }
            
            var cart = await GetOrCreateCartAsync(userId);
            
            var existingItem = await context.CartItems.FirstOrDefaultAsync(
                ci => ci.CartId == cart.Id && ci.ProductId == dto.ProductId && ci.ProductVariantId == dto.ProductVariantId);
            
            var requestedTotalQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;
            if (requestedTotalQuantity > availableStock)
                return new Response<string>(HttpStatusCode.BadRequest,
                    $"Танҳо {availableStock} дона дар анбор мондааст");

            if (existingItem != null)
            {
                existingItem.Quantity = requestedTotalQuantity;
            }
            else
            {
                context.CartItems.Add(new CartItem()
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity,
                });
            }
            cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return new Response<string>(HttpStatusCode.OK, "Ба сабад илова шуд");
        }
        catch (Exception e)
        {
            Log.ForContext<CartService>().Error(e, "AddToCartAsync failed for user {UserId} product {ProductId}", userId, dto.ProductId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
    {
        Log.ForContext<CartService>().Information("UpdateCartItemAsync started for user {UserId} cartItem {CartItemId} quantity {Quantity}", userId, cartItemId, dto.Quantity);
        try
        {
            var item = await context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .Include(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);
            
            if (item == null)
                return new Response<string>(HttpStatusCode.NotFound,"Ин ашё дар сабад ёфт нашуд");
            
            if (dto.Quantity <= 0)
                return new Response<string>(HttpStatusCode.BadRequest,"Миқдор бояд аз сифр зиёд бошад");
            
            var availableStock = item.ProductVariant?.StockQuantity ?? item.Product.StockQuantity;
            if (dto.Quantity > availableStock)
                return new Response<string>(HttpStatusCode.BadRequest,$"Танҳо {availableStock} дона дар анбор мондааст");
            
            item.Quantity = dto.Quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return new Response<string>(HttpStatusCode.OK,"Cart updated");
        }
        catch (Exception e)
        {
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> RemoveFromCartAsync(int userId, int cartItemId)
    {
        try
        {
            Log.Information("Removing cart item {CartItemId} for user {UserId}", cartItemId, userId);
            var item = await context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);
            
            if (item == null)
                return new Response<string>(HttpStatusCode.NotFound,"Ин ашё дар сабад ёфт нашуд");
            
            var cartId = item.CartId;
            context.CartItems.Remove(item);
            await context.SaveChangesAsync();
            Log.Information("Cart item {CartItemId} removed from cart {CartId}", cartItemId, cartId);
            return new Response<string>(HttpStatusCode.OK, "Item removed");
        }
        catch (Exception e)
        {
            Log.Error(e, "RemoveFromCartAsync failed for cart item {CartItemId} and user {UserId}", cartItemId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<CartDto>> GetCartItemsAsync(int userId)
    {
        try
        {
            Log.Information("Retrieving cart items for user {UserId}", userId);
            var cart = await GetOrCreateCartAsync(userId);
            var cartDto = await MapAsync(cart.Id);
            Log.Information("Retrieved {ItemCount} cart items for user {UserId}", cartDto.Items.Count, userId);
            return new Response<CartDto>(cartDto);
        }
        catch (Exception e)
        {
            Log.Error(e, "GetCartItemsAsync failed for user {UserId}", userId);
            return new Response<CartDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }    }

    public async Task<Response<string>> ClearCartAsync(int userId)
    {
        try
        {
            Log.Information("Clearing cart for user {UserId}", userId);
            var cart = await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart?.Items != null && cart.Items.Count != 0)
            {
                context.CartItems.RemoveRange(cart.Items);
                await context.SaveChangesAsync();
            }
            Log.Information("Cart cleared for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.OK, "Cart cleared");
        }
        catch (Exception e)
        {
            Log.Error(e, "ClearCartAsync failed for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
    
    private async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart != null)
            return cart;

        cart = new Cart { UserId = userId, UpdatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        await context.SaveChangesAsync();
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

        var itemDtos = items.Select(ci =>
        {
            var unitPrice = (ci.Product.DiscountPrice ?? ci.Product.Price) + (ci.ProductVariant?.ExtraPrice ?? 0);
            return new Domain.DTOs.CartDto.CartItemDto(
                ci.Id,
                ci.ProductId,
                ci.Product.Name,
                ci.Product.Images?.FirstOrDefault()?.Url,
                unitPrice,
                ci.Quantity,
                unitPrice * ci.Quantity,
                ci.ProductVariantId,
                ci.ProductVariant != null ? $"{ci.ProductVariant.Name}: {ci.ProductVariant.Value}" : null
            );
        }).ToList();

        return new CartDto(cartId, itemDtos, itemDtos.Sum(i => i.TotalPrice));
    }
}