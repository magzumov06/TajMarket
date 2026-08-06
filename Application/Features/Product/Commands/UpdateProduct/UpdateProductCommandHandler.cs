using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler(
    IApplicationDbContext context,
    ILogger<UpdateProductCommandHandler> logger)
    : IRequestHandler<UpdateProductCommand, Response<string>>
{
    public async Task<Response<string>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var (sellerUserId, productId, dto) = (request.SellerUserId, request.ProductId, request.Dto);

        try
        {
            logger.LogInformation("Updating product {ProductId} for seller {SellerUserId}", productId, sellerUserId);

            var product = await context.Products
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product == null)
            {
                logger.LogWarning("Product not found {ProductId}", productId);
                return new Response<string>(HttpStatusCode.NotFound, "Product not found");
            }

            if (product.SellerProfile.UserId != sellerUserId)
            {
                logger.LogWarning("Seller {SellerUserId} tried to update product {ProductId} without permission", sellerUserId, productId);
                return new Response<string>(HttpStatusCode.Forbidden, "You don't have the seller profile");
            }

            if (dto.DiscountPrice.HasValue && dto.DiscountPrice >= dto.Price)
            {
                logger.LogWarning("Invalid discount price for product {ProductId}", productId);
                return new Response<string>(HttpStatusCode.Forbidden, "Price must be greater than DiscountPrice");
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.DiscountPrice = dto.DiscountPrice;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product updated successfully: {ProductId}", productId);

            return new Response<string>(HttpStatusCode.OK, "Product Updated successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating product {ProductId} for seller {SellerUserId}", productId, sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}