using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Product.Commands.DeleteProduct;

public class DeleteProductCommandHandler(
    IApplicationDbContext context,
    ILogger<DeleteProductCommandHandler> logger)
    : IRequestHandler<DeleteProductCommand, Response<string>>
{
    public async Task<Response<string>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var (sellerUserId, productId) = (request.SellerUserId, request.ProductId);

        try
        {
            logger.LogInformation("Deleting product {ProductId} for seller {SellerUserId}", productId, sellerUserId);

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
                logger.LogWarning("Seller {SellerUserId} has no permission to delete product {ProductId}", sellerUserId, productId);
                return new Response<string>(HttpStatusCode.Forbidden, "You don't have the seller profile");
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product deleted successfully: {ProductId}", productId);

            return new Response<string>(HttpStatusCode.OK, "Product Deleted successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting product {ProductId} for seller {SellerUserId}", productId, sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}