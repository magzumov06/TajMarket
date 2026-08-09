using Application.Features.Product;
using Application.Features.Product.Commands.CreateProduct;
using Application.Features.Product.Commands.DeleteProduct;
using Application.Features.Product.Commands.UpdateProduct;
using Application.Features.Product.Commands.UploadProductImages;
using Application.Features.Product.DTOs;
using Application.Features.Product.Queries.GetProductDetail;
using Application.Features.Product.Queries.GetProductList;
using Application.Features.Product.Queries.GetProductsBySeller;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class ProductController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var res = await mediator.Send(new CreateProductCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPost("{productId}/images")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UploadImages(int productId, [FromForm] IFormFileCollection files)
    {
        var res = await mediator.Send(new UploadProductImagesCommand(UserId, productId, files));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPut("{productId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateProduct(int productId, [FromBody] UpdateProductDto dto)
    {
        var res = await mediator.Send(new UpdateProductCommand(UserId, productId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpDelete("{productId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        var res = await mediator.Send(new DeleteProductCommand(UserId, productId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductDetail(int productId)
    {
        var res = await mediator.Send(new GetProductDetailQuery(productId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("seller/my-products")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetMyProducts([FromQuery] ProductFilter filter)
    {
        var res = await mediator.Send(new GetProductsBySellerQuery(UserId, filter));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductList([FromQuery] ProductFilter filter)
    {
        var res = await mediator.Send(new GetProductListQuery(filter));
        return StatusCode((int)res.StatusCode, res);
    }
}