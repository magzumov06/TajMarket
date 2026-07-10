using System.Net;
using Domain.DTOs.CategoryDtos;
using Domain.DTOs.ProductDto;
using Domain.DTOs.ReviewDtos;
using Domain.DTOs.SellerDto;
using Domain.Entities.ProductEntity;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class ProductService(DataContext context,
    IFileStorageService fileStorage) : IProductService
{
    #region CreateProduct
    public async Task<Response<string>> CreateProductAsync(int sellerUserId, CreateProductDto dto)
    {
        try
        {
            Log.Information("Creating product for seller {SellerUserId}: {ProductName}", sellerUserId, dto.Name);
            var sellerProfil = await context.SellerProfiles.FirstOrDefaultAsync(sp => sp.UserId == sellerUserId);
            if (sellerProfil == null)
                return new Response<string>(HttpStatusCode.BadRequest, "Аввал бояд ҳамчун Seller сабт шавед");
            
            var categoryExist = await context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExist)
                return new Response<string>(HttpStatusCode.BadRequest, "Category not found");
            
            if(dto.Price <= 0)
                return new Response<string>(HttpStatusCode.BadRequest, "Price must be greater than 0");

            if (dto.DiscountPrice.HasValue && dto.DiscountPrice >= dto.Price)
                return new Response<string>(HttpStatusCode.BadRequest, "DiscountPrice must be less than Price");

            var basesSlug = SlugHelper.GenerateSlug(dto.Name);
            var slug = await context.Products.AnyAsync(p=> p.Slug == basesSlug)
                ? SlugHelper.GenerateSlug(basesSlug)
                : basesSlug;

            var product = new Product
            {
                Name = dto.Name,
                Slug = slug,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                SellerProfileId = sellerProfil.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Images = new List<ProductImage>(),
                Variants = dto.Variants?.Select(v => new ProductVariant
                           {
                               Name = v.Name,
                               Value = v.Value,
                               ExtraPrice = v.ExtraPrice,
                               StockQuantity = v.StockQuantity,
                           }).ToList()
                           ?? new List<ProductVariant>()
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();
            Log.Information("Product created successfully: {ProductId} for seller {SellerUserId}", product.Id, sellerUserId);
            return new Response<string>(HttpStatusCode.OK, "Product Added successfully");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error creating product for seller {SellerUserId}", sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
    #endregion

    #region UpdateProduct
    public async Task<Response<string>> UpdateProductAsync(int sellerUserId, int productId, UpdateProductDto dto)
    {
        try
        {
            Log.Information("Updating product {ProductId} for seller {SellerUserId}", productId, sellerUserId);
            var product = await context.Products
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return new Response<string>(HttpStatusCode.NotFound, "Product not found");
            
            if(product.SellerProfile.UserId != sellerUserId)
                return new Response<string>(HttpStatusCode.Forbidden, "You don't have the seller profile");
            
            if(dto.DiscountPrice.HasValue && dto.DiscountPrice >= dto.Price)
                return new Response<string>(HttpStatusCode.Forbidden, "Price must be greater than DiscountPrice");
            
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.DiscountPrice = dto.DiscountPrice;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            
            await context.SaveChangesAsync();
            Log.Information("Product updated successfully: {ProductId}", productId);
            return new Response<string>(HttpStatusCode.OK, "Product Updated successfully");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error updating product {ProductId} for seller {SellerUserId}", productId, sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
    #endregion

    #region DeleteProduct
    public async Task<Response<string>> DeleteProductAsync(int sellerUserId, int productId)
    {
        try
        {
            Log.Information("Deleting product {ProductId} for seller {SellerUserId}", productId, sellerUserId);
            var product = await context.Products
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null)
                return new Response<string>(HttpStatusCode.NotFound, "Product not found");
            
            if (product.SellerProfile.UserId != sellerUserId)
                return new Response<string>(HttpStatusCode.Forbidden, "You don't have the seller profile");
            
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            Log.Information("Product deleted successfully: {ProductId}", productId);
            return new Response<string>(HttpStatusCode.OK, "Product Deleted successfully");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error deleting product {ProductId} for seller {SellerUserId}", productId, sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
    #endregion
    
    #region UploadProductImage
    public async Task<Response<string>> UploadImageProductAsync(int sellerUserId, int productId, IEnumerable<IFormFile> files)
    {
        try
        {
            Log.Information("Uploading product images for product {ProductId} and seller {SellerUserId}", productId, sellerUserId);
            var product = await context.Products
                .Include(p => p.SellerProfile)
                .Include(product => product.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null)
                return new Response<string>(HttpStatusCode.NotFound, "Product not found");

            if (product.SellerProfile.UserId != sellerUserId)
                return new Response<string>(HttpStatusCode.Forbidden, "You don't have the seller profile");

            var upload = await fileStorage.UploadImagesAsync(files, "products");
            var hasMainAlready = product.Images.Any(i => i.IsMain);
            var startOrder = product.Images.Count;
            
            var newIamges = upload.Select((r, index) => new ProductImage
            {
               ProductId = productId,
               Url = r.Url,
               PublicId = r.PublicId,
               IsMain = !hasMainAlready && index == 0,
               SortOrder = startOrder +  index,
            }).ToList();
            
            context.ProductImages.AddRange(newIamges);
            await context.SaveChangesAsync();
            Log.Information("Uploaded {ImageCount} images for product {ProductId}", newIamges.Count, productId);
            return new Response<string>(HttpStatusCode.OK, "Product Images Uploaded");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error uploading images for product {ProductId} by seller {SellerUserId}", productId, sellerUserId);
            return new Response<string>(HttpStatusCode.InternalServerError,"Internal Server Error");
        }
    }
    #endregion
    
    #region GetDetail
    public async Task<Response<ProductDetailDto>> GetDetailAsync(int id)
    {
        try
        {
            Log.Information("Retrieving product details for product {ProductId}", id);
            var product = await context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Include(p => p.SellerProfile)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id &&  p.IsActive);
            if (product == null)
                return new Response<ProductDetailDto>(HttpStatusCode.NotFound, "Product not found");
            
            var reviews = product.Reviews
                .OrderByDescending(p => p.CreatedAt)
                .Select(r=> new ReviewDto(r.Id, r.User.FullName, r.User.AvatarUrl,r.Rating,r.Comment,r.CreatedAt))
                .ToList();

            var dto = new ProductDetailDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.DiscountPrice,
                product.StockQuantity,
                product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
                product.Variants
                    .Select(v => new ProductVariantDto(v.Id, v.Name, v.Value, v.ExtraPrice, v.StockQuantity)).ToList(),
                new CategoryDto(product.Category.Id, product.Category.Name, product.Category.Slug,
                    product.Category.IconUrl, product.Category.ParentCategoryId, null),
                new SellerProfileDto(product.SellerProfile.Id, product.SellerProfile.StoreName,
                    product.SellerProfile.StoreDescription,
                    product.SellerProfile.StoreLogoUrl, product.SellerProfile.IsVerified, product.SellerProfile.Rating,
                    0),
                product.Reviews.Count != 0
                    ? decimal.Round(product.Reviews.Average(r => (decimal)r.Rating), 1)
                    : 0m,
            reviews
            );
            Log.Information("Product details retrieved successfully for product {ProductId}", id);
            return new Response<ProductDetailDto>(dto);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error retrieving product details for product {ProductId}", id);
            return new Response<ProductDetailDto>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
    #endregion

    #region GetProductList
    public async Task<PaginationResponse<List<ProductListDto>>> GetProductListAsync(ProductFilter filter)
    {
        try
        {
            Log.Information("Retrieving product list with filter {@Filter}", filter);
            var query= context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Include(p => p.SellerProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) ||  p.Description.ToLower().Contains(term));
            }
            
            if(filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);
            
            if(filter.MinPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice ?? p.Price) >= filter.MinPrice);
            
            if(filter.MaxPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice ?? p.Price) <= filter.MaxPrice);

            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "rating" => query.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
            
            var totalCount = await query.CountAsync();
            
            var products =  await query
                .Skip((filter.PageNumber - 1) *filter.PageSize) 
                .Take(filter.PageSize)
                .ToListAsync();
            
            var items = products
                .Select(ToListDto)
                .Where(p => !filter.MinRating.HasValue || p.AverageRating >= filter.MinRating)
                .ToList();
            Log.Information("Product list retrieved: {ItemCount} items, total {TotalCount}", items.Count, totalCount);
            return new PaginationResponse<List<ProductListDto>>(items, totalCount, filter.PageNumber, filter.PageSize);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error retrieving product list with filter {@Filter}", filter);
            return new PaginationResponse<List<ProductListDto>>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
    #endregion

    #region GetBySellerId
    public async Task<Response<List<ProductListDto>>> GetBySellerAsync(int sellerUserId)
    {
        try
        {
            Log.Information("Retrieving products for seller {SellerUserId}", sellerUserId);
            var sellerProfile = await context.SellerProfiles.FirstOrDefaultAsync(sp => sp.UserId == sellerUserId);
            if (sellerProfile == null)
                return new Response<List<ProductListDto>>(HttpStatusCode.NotFound, "Seller profile not  found");
            
            var products = await context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Include(p => p.SellerProfile)
                .Where(p=> p.SellerProfileId == sellerProfile.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            
            Log.Information("Retrieved {ProductCount} products for seller {SellerUserId}", products.Count, sellerUserId);
            return new Response<List<ProductListDto>>(products.Select(ToListDto).ToList());
        }
        catch (Exception e)
        {
            Log.Error(e, "Error retrieving products for seller {SellerUserId}", sellerUserId);
            return new Response<List<ProductListDto>>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
    #endregion
    
    private static ProductListDto ToListDto(Product p) => new(
        p.Id,
        p.Name,
        p.Slug,
        p.Price,
        p.DiscountPrice,
        p.Images.FirstOrDefault(i => i.IsMain)?.Url ?? p.Images.FirstOrDefault()?.Url,
        p.Reviews.Count != 0 
            ? decimal.Round(p.Reviews.Average(r => (decimal)r.Rating), 1) 
            : 0,
        p.Reviews.Count,
        p.SellerProfile.StoreName
    );
}