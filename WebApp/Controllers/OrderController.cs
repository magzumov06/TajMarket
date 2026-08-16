using Application.Features.Order;
using Application.Features.Order.Commands.ApproveReturnRequest;
using Application.Features.Order.Commands.CancelOrder;
using Application.Features.Order.Commands.CompleteOrder;
using Application.Features.Order.Commands.CompleteReturn;
using Application.Features.Order.Commands.CreateOrder;
using Application.Features.Order.Commands.CreateReturnRequest;
using Application.Features.Order.Commands.RejectReturnRequest;
using Application.Features.Order.Commands.UpdateOrderStatus;
using Application.Features.Order.Dtos;
using Application.Features.Order.DTOs;
using Application.Features.Order.Queries.CalculateTotalPrice;
using Application.Features.Order.Queries.GetMyReturnRequests;
using Application.Features.Order.Queries.GetOrderDetail;
using Application.Features.Order.Queries.GetOrderList;
using Application.Features.Order.Queries.GetOrdersBySeller;
using Application.Features.Order.Queries.GetPendingReturnRequests;
using Application.Features.Order.Queries.GetReturnRequestById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class OrderController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var res = await mediator.Send(new CreateOrderCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPost("{orderId}/complete")]
    [Authorize]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
        var res = await mediator.Send(new CompleteOrderCommand(UserId, orderId));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPost("calculate-total")]
    [Authorize]
    public async Task<IActionResult> CalculateTotalPrice([FromBody] CalculateTotalPriceDto dto)
    {
        var res = await mediator.Send(new CalculateTotalPriceQuery(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPost("{orderId}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var res = await mediator.Send(new CancelOrderCommand(UserId, orderId));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPost("returns")]
    [Authorize]
    public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestDto dto)
    {
        var res = await mediator.Send(new CreateReturnRequestCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPut("{orderId}/status")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var res = await mediator.Send(new UpdateOrderStatusCommand(UserId, orderId, dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPut("returns/{returnRequestId}/approve")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> ApproveReturnRequest(int returnRequestId, [FromBody] ResolveReturnRequestDto dto)
    {
        var res = await mediator.Send(new ApproveReturnRequestCommand(returnRequestId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPut("returns/{returnRequestId}/reject")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> RejectReturnRequest(int returnRequestId, [FromBody] ResolveReturnRequestDto dto)
    {
        var res = await mediator.Send(new RejectReturnRequestCommand(returnRequestId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPut("returns/{returnRequestId}/complete")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> CompleteReturn(int returnRequestId)
    {
        var res = await mediator.Send(new CompleteReturnCommand(returnRequestId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderList([FromQuery] OrderFilter filter)
    {
        var res = await mediator.Send(new GetOrderListQuery(UserId, filter));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderDetail(int orderId)
    {
        var res = await mediator.Send(new GetOrderDetailQuery(orderId, UserId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("seller/orders")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerOrders([FromQuery] OrderFilter filter)
    {
        var res = await mediator.Send(new GetOrdersBySellerQuery(UserId, filter));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpGet("returns/my")]
    [Authorize]
    public async Task<IActionResult> GetMyReturnRequests()
    {
        var res = await mediator.Send(new GetMyReturnRequestsQuery(UserId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("returns/{returnRequestId}")]
    [Authorize]
    public async Task<IActionResult> GetReturnRequestById(int returnRequestId)
    {
        var res = await mediator.Send(new GetReturnRequestByIdQuery(returnRequestId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("returns/pending")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> GetPendingReturnRequests()
    {
        var res = await mediator.Send(new GetPendingReturnRequestsQuery());
        return StatusCode((int)res.StatusCode, res);
    }
}