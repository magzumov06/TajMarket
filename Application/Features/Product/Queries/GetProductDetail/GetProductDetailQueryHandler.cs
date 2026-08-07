using System.Net;
using Application.Common.Interfaces;
using Application.Features.Category.DTOs;
using Application.Features.Product.DTOs;
using Application.Features.Review.DTOs;
using Application.Features.Seller.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Product.Queries.GetProductDetail;

public class GetProductDetailQueryHandler(
    IApplicationDbContext context,
    ILogger<GetProductDetailQueryHandler> logger)
    : IRequestHandler<GetProductDetailQuery, Response<ProductDetailDto>>
{
    public async Task<Response<ProductDetailDto>> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        var productId = request.ProductId;

        try
        {
            logger.LogInformation("Retrieving product detail {ProductId}", productId);

            var product = await context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Include(p => p.SellerProfile)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive, cancellationToken);

            if (product == null)
            {
                logger.LogWarning("Product not found {ProductId}", productId);
                return new Response<ProductDetailDto>(HttpStatusCode.NotFound, "Product not found");
            }

            // ← БАГ ИСЛОҲ ШУД: пеш аз ин "0" сахт-рамзгузорӣ буд
            var totalSellerProducts = await context.Products
                .CountAsync(p => p.SellerProfileId == product.SellerProfileId && p.IsActive, cancellationToken);

            var dto = new ProductDetailDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.DiscountPrice,
                product.StockQuantity,

                product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),

                product.Variants.Select(v => new ProductVariantDto(
                    v.Id, v.Name, v.Value, v.ExtraPrice, v.StockQuantity)).ToList(),

                new CategoryDto(
                    product.Category.Id,
                    product.Category.Name,
                    product.Category.Slug,
                    product.Category.IconUrl,
                    product.Category.ParentCategoryId,
                    new List<CategoryDto>()),

                new SellerProfileDto(
                    product.SellerProfile.Id,
                    product.SellerProfile.StoreName,
                    product.SellerProfile.StoreDescription,
                    product.SellerProfile.StoreLogoUrl,
                    product.SellerProfile.IsVerified,
                    product.SellerProfile.Rating,
                    totalSellerProducts),   // ← ислоҳшуда

                product.Reviews.Count != 0
                    ? decimal.Round(product.Reviews.Average(r => (decimal)r.Rating), 1)
                    : 0,

                product.Reviews.Select(r => new ReviewDto(
                    r.Id,
                    r.User.FullName,
                    r.User.AvatarUrl,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt)).ToList()
            );

            logger.LogInformation("Product detail retrieved successfully {ProductId}", productId);

            return new Response<ProductDetailDto>(dto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving product detail {ProductId}", productId);
            return new Response<ProductDetailDto>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}