using System.Net;
using Application.Common.Interfaces;
using Application.Common.Utils;
using Domain.Entities.ProductEntity;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Product.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, Response<string>>
{
    public async Task<Response<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var (sellerUserId, dto) = (request.SellerUserId, request.Dto);

        try
        {
            logger.LogInformation("Creating product for seller {SellerUserId}: {ProductName}", sellerUserId, dto.Name);

            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == sellerUserId, cancellationToken);

            if (sellerProfile == null)
            {
                logger.LogWarning("Seller profile not found for user {SellerUserId}", sellerUserId);
                return new Response<string>(HttpStatusCode.BadRequest, "Аввал бояд ҳамчун Seller сабт шавед");
            }

            var categoryExist = await context.Categories.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);

            if (!categoryExist)
            {
                logger.LogWarning("Category not found {CategoryId}", dto.CategoryId);
                return new Response<string>(HttpStatusCode.NotFound, "Category not found");
            }

            if (dto.Price <= 0)
            {
                logger.LogWarning("Invalid product price {Price}", dto.Price);
                return new Response<string>(HttpStatusCode.BadRequest, "Price must be greater than 0");
            }

            if (dto.DiscountPrice.HasValue && dto.DiscountPrice >= dto.Price)
            {
                logger.LogWarning("Invalid discount price {DiscountPrice} for price {Price}", dto.DiscountPrice, dto.Price);
                return new Response<string>(HttpStatusCode.BadRequest, "DiscountPrice must be less than Price");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Response<string>(HttpStatusCode.BadRequest, "Product name is required");

            var baseSlug = SlugHelper.GenerateBase(dto.Name);
            var slug = baseSlug;
            var attempt = 0;

            while (await context.Products.AnyAsync(p => p.Slug == slug, cancellationToken))
            {
                attempt++;
                slug = SlugHelper.WithSuffix(baseSlug, attempt);
            }

            var product = new Domain.Entities.ProductEntity.Product
            {
                Name = dto.Name,
                Slug = slug,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                SellerProfileId = sellerProfile.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Images = new List<ProductImage>(),
                Variants = dto.Variants?.Select(v => new ProductVariant
                {
                    Name = v.Name,
                    Value = v.Value,
                    ExtraPrice = v.ExtraPrice,
                    StockQuantity = v.StockQuantity,
                }).ToList() ?? new List<ProductVariant>()
            };

            context.Products.Add(product);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product created successfully: {ProductId} for seller {SellerUserId}", product.Id, sellerUserId);

            return new Response<string>(HttpStatusCode.Created, "Product Added successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating product for seller {SellerUserId}", sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}