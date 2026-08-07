using System.Net;
using Application.Common.Interfaces;
using Application.Features.Product.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Product.Queries.GetProductsBySeller;

public class GetProductsBySellerQueryHandler(
    IApplicationDbContext context,
    ILogger<GetProductsBySellerQueryHandler> logger)
    : IRequestHandler<GetProductsBySellerQuery, PaginationResponse<List<ProductListDto>>>
{
    public async Task<PaginationResponse<List<ProductListDto>>> Handle(GetProductsBySellerQuery request, CancellationToken cancellationToken)
    {
        var (sellerUserId, filter) = (request.SellerUserId, request.Filter);

        try
        {
            logger.LogInformation("Retrieving products for seller user {SellerUserId}", sellerUserId);

            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == sellerUserId, cancellationToken);

            if (sellerProfile == null)
            {
                logger.LogWarning("Seller profile not found for user {SellerUserId}", sellerUserId);
                return new PaginationResponse<List<ProductListDto>>(HttpStatusCode.NotFound, "Seller profile not found");
            }

            var query = context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Include(p => p.SellerProfile)
                .Where(p => p.SellerProfileId == sellerProfile.Id)
                .AsQueryable();

            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {ProductCount} products for seller {SellerUserId}", products.Count, sellerUserId);

            return new PaginationResponse<List<ProductListDto>>(
                products.Select(ProductMapper.ToListDto).ToList(),
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving products for seller {SellerUserId}", sellerUserId);
            return new PaginationResponse<List<ProductListDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}