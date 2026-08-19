using Application.Features.Payment.Commands.ProcessPayment;
using Application.Features.Payment.DTOs;
using Application.Features.Payment.Queries.GetPaymentByOrderId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class PaymentController(IMediator mediator) : BaseApiController
{
    [HttpPost("process")]
    [Authorize]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        var res = await mediator.Send(new ProcessPaymentCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("order/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentByOrder(int orderId)
    {
        var res = await mediator.Send(new GetPaymentByOrderIdQuery(UserId, orderId));
        return StatusCode((int)res.StatusCode, res);
    }
}