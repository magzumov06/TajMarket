using System.Collections;
using Domain.DTOs.ProductDto;
using Domain.Filters;
using Domain.Respoces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces;

public interface IProductService
{
    Task<Responce<string>> CreateProductAsync(int sellerUserId, CreateProductDto dto);
    Task<Responce<string>> UpdateProductAsync(int sellerUserId, int productId, UpdateProductDto dto);
    Task<Responce<string>> DeleteProductAsync(int sellerUserId, int productId);
    Task<Responce<string>> UploadImageProductAsync(int sellerUserId, int productId, IEnumerable<IFormFile> files);
    Task<Responce<ProductDetailDto>> GetDetailAsync(int id);
    Task<PaginationResponce<List<ProductListDto>>> GetProductListAsync(ProductFilter filter);
    Task<Responce<List<ProductListDto>>> GetBySellerAsync(int sellerUserId);
}