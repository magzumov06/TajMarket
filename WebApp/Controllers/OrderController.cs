using Domain.DTOs.OrderDto;
using Domain.Filters;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class OrderController(IOrderService orderService) : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var res = await orderService.CreateOrderAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPost("{orderId}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var res = await orderService.CancelOrderAsync(UserId, orderId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderList([FromQuery] OrderFilter filter)
    {
        var res = await orderService.GetOrderListAsync(UserId, filter);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderDetail(int orderId)
    {
        var res = await orderService.GetOrderDetailAsync(orderId, UserId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("seller/orders")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerOrders([FromQuery] OrderFilter filter)
    {
        var res = await orderService.GetBySellerIdAsync(UserId, filter);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("{orderId}/status")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var res = await orderService.UpdateStatusAsync(UserId, orderId, dto);
        return StatusCode((int)res.StatusCode, res);
    }
}