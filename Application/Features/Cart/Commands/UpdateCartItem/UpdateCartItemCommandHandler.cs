using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler(
    IApplicationDbContext context,
    ILogger<UpdateCartItemCommandHandler> logger)
    : IRequestHandler<UpdateCartItemCommand, Response<string>>
{
    public async Task<Response<string>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var (userId, cartItemId, dto) = (request.UserId, request.CartItemId, request.Dto);

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
                        ci.Cart.UserId == userId,
                    cancellationToken);

            if (item == null)
            {
                logger.LogWarning("Cart item not found {CartItemId} for user {UserId}", cartItemId, userId);
                return new Response<string>(HttpStatusCode.NotFound, "Ин ашё дар сабад ёфт нашуд");
            }

            if (dto.Quantity <= 0)
                return new Response<string>(HttpStatusCode.BadRequest, "Миқдор бояд аз сифр зиёд бошад");

            var availableStock = item.ProductVariant?.StockQuantity ?? item.Product.StockQuantity;

            if (dto.Quantity > availableStock)
                return new Response<string>(HttpStatusCode.BadRequest, $"Танҳо {availableStock} дона дар анбор мондааст");

            item.Quantity = dto.Quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Cart item updated {CartItemId} for user {UserId}", cartItemId, userId);

            return new Response<string>(HttpStatusCode.OK, "Cart updated");
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateCartItemAsync failed for cart item {CartItemId} user {UserId}", cartItemId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}