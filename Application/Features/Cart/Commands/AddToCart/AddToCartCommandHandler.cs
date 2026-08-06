using System.Net;
using Application.Common.Interfaces;
using Domain.Entities.CartEntity;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cart.Commands.AddToCart;

public class AddToCartCommandHandler(
    IApplicationDbContext context,
    ILogger<AddToCartCommandHandler> logger)
    : IRequestHandler<AddToCartCommand, Response<string>>
{
    public async Task<Response<string>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        logger.LogInformation(
            "AddToCartAsync started for user {UserId} product {ProductId} variant {ProductVariantId} quantity {Quantity}",
            userId, dto.ProductId, dto.ProductVariantId, dto.Quantity);

        try
        {
            var product = await context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive, cancellationToken);

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

            var cart = await CartHelper.GetOrCreateCartAsync(context, userId, logger, cancellationToken);

            var existingItem = await context.CartItems.FirstOrDefaultAsync(
                ci =>
                    ci.CartId == cart.Id &&
                    ci.ProductId == dto.ProductId &&
                    ci.ProductVariantId == dto.ProductVariantId,
                cancellationToken);

            var requestedTotalQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;

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

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product {ProductId} added to cart for user {UserId}", dto.ProductId, userId);

            return new Response<string>(HttpStatusCode.OK, "Ба сабад илова шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "AddToCartAsync failed for user {UserId} product {ProductId}", userId, dto.ProductId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}