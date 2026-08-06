using Application.Features.Product;
using Application.Features.Product.DTOs;
using Domain.DTOs.ProductDto;
using Domain.Filters;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebApp.Controllers;


public class ProductController(IProductService productService) : BaseApiController
{
    
    [HttpPost]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var res = await productService.CreateProductAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("{productId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateProduct(int productId, [FromBody] UpdateProductDto dto)
    {
        var res = await productService.UpdateProductAsync(UserId, productId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpDelete("{productId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        var res = await productService.DeleteProductAsync(UserId, productId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPost("{productId}/images")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UploadProductImages(int productId, [FromForm] IFormFileCollection files)
    {
        var res = await productService.UploadImageProductAsync(UserId, productId, files);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductDetail(int id)
    {
        var res = await productService.GetDetailAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductList([FromQuery] ProductFilter filter)
    {
        var res = await productService.GetProductListAsync(filter);
        return Ok(res);
    }

    
    [HttpGet("seller/products")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerProducts()
    {
        var res = await productService.GetBySellerAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }
}
