using Domain.DTOs.OrderDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class OrderController(IOrderService orderService) : BaseApiController
{
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var res = await orderService.CreateOrderAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var res = await orderService.CancelOrderAsync(UserId, orderId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet]
    public async Task<IActionResult> GetOrderList()
    {
        var res = await orderService.GetOrderListAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetail(int orderId)
    {
        var res = await orderService.GetOrderDetailAsync(orderId, UserId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("seller/orders")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> GetSellerOrders()
    {
        var res = await orderService.GetBySellerIdAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("{orderId}/status")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var res = await orderService.UpdateStatusAsync(orderId, dto);
        return StatusCode((int)res.StatusCode, res);
    }
}
