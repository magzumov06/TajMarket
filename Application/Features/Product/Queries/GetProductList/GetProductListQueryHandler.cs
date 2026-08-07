using Application.Common.Interfaces;
using Application.Features.Product.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Product.Queries.GetProductList;

public class GetProductListQueryHandler(
    IApplicationDbContext context,
    ILogger<GetProductListQueryHandler> logger)
    : IRequestHandler<GetProductListQuery, PaginationResponse<List<ProductListDto>>>
{
    public async Task<PaginationResponse<List<ProductListDto>>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        try
        {
            logger.LogInformation("Retrieving product list with filter {@Filter}", filter);

            var query = context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Include(p => p.SellerProfile)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(p =>
                    p.Name.Contains(filter.SearchTerm) ||
                    p.Description.Contains(filter.SearchTerm));

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice ?? p.Price) >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice ?? p.Price) <= filter.MaxPrice.Value);

            if (filter.MinRating.HasValue)
                query = query.Where(p =>
                    p.Reviews.Count != 0 &&
                    p.Reviews.Average(r => (double)r.Rating) >= (double)filter.MinRating.Value);

            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {ProductCount} products, total {TotalCount}", products.Count, totalCount);

            return new PaginationResponse<List<ProductListDto>>(
                products.Select(ProductMapper.ToListDto).ToList(),
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving product list");
            throw;
        }
    }
}