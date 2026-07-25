using Domain.DTOs.PaymentDtos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebApp.Controllers;


public class PaymentController(IPaymentService paymentService) : BaseApiController
{
    
    [HttpPost("process")]
    [Authorize]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        var res = await paymentService.ProcessAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("order/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentByOrder(int orderId)
    {
        var res = await paymentService.GetByOrderIdAsync(UserId, orderId);
        return StatusCode((int)res.StatusCode, res);
    }
}
