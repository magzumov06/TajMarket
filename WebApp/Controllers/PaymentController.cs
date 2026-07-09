using Domain.DTOs.PaymentDtos;
using Domain.Responses;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");


    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        var res = await paymentService.ProcessAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetPaymentByOrder(int orderId)
    {
        var res = await paymentService.GetByOrderIdAsync(UserId, orderId);
        return StatusCode((int)res.StatusCode, res);
    }
}
