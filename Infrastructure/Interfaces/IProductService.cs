using Application.Features.Product;
using Application.Features.Product.DTOs;
using Domain.DTOs.ProductDto;
using Domain.Filters;
using Domain.Responses;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces;

public interface IProductService
{
    Task<Response<string>> CreateProductAsync(int sellerUserId, CreateProductDto dto);
    Task<Response<string>> UpdateProductAsync(int sellerUserId, int productId, UpdateProductDto dto);
    Task<Response<string>> DeleteProductAsync(int sellerUserId, int productId);
    Task<Response<string>> UploadImageProductAsync(int sellerUserId, int productId, IEnumerable<IFormFile> files);
    Task<Response<ProductDetailDto>> GetDetailAsync(int id);
    Task<PaginationResponse<List<ProductListDto>>> GetProductListAsync(ProductFilter filter);
    Task<Response<List<ProductListDto>>> GetBySellerAsync(int sellerUserId);
}