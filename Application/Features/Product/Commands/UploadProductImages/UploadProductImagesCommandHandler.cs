using System.Net;
using Application.Common.Interfaces;
using Domain.Entities.ProductEntity;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Product.Commands.UploadProductImages;

public class UploadProductImagesCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorage,
    ILogger<UploadProductImagesCommandHandler> logger)
    : IRequestHandler<UploadProductImagesCommand, Response<string>>
{
    public async Task<Response<string>> Handle(UploadProductImagesCommand request, CancellationToken cancellationToken)
    {
        var (sellerUserId, productId, files) = (request.SellerUserId, request.ProductId, request.Files);

        try
        {
            logger.LogInformation("Uploading product images for product {ProductId} and seller {SellerUserId}", productId, sellerUserId);

            var product = await context.Products
                .Include(p => p.SellerProfile)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product == null)
            {
                logger.LogWarning("Product not found {ProductId}", productId);
                return new Response<string>(HttpStatusCode.NotFound, "Product not found");
            }

            if (product.SellerProfile.UserId != sellerUserId)
            {
                logger.LogWarning("Seller {SellerUserId} has no permission to upload images for product {ProductId}", sellerUserId, productId);
                return new Response<string>(HttpStatusCode.Forbidden, "You don't have the seller profile");
            }

            var upload = await fileStorage.UploadImagesAsync(files, "products");

            var hasMainAlready = product.Images.Any(i => i.IsMain);
            var startOrder = product.Images.Count;

            var newImages = upload.Select((r, index) => new ProductImage
            {
                ProductId = productId,
                Url = r.Url,
                PublicId = r.PublicId,
                IsMain = !hasMainAlready && index == 0,
                SortOrder = startOrder + index,
            }).ToList();

            context.ProductImages.AddRange(newImages);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Uploaded {ImageCount} images for product {ProductId}", newImages.Count, productId);

            return new Response<string>(HttpStatusCode.OK, "Product Images Uploaded");
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Invalid file(s) for product {ProductId}", productId);
            return new Response<string>(HttpStatusCode.BadRequest, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error uploading images for product {ProductId} by seller {SellerUserId}", productId, sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}