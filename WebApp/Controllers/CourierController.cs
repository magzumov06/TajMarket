using Domain.DTOs.CourierDto;
using Domain.Filters;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class CouriersController(ICourierService courierService) : BaseApiController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCourier([FromBody] CreateCourierDto dto)
    {
        var res = await courierService.CreateCourierAsync(dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCourier(int id, [FromBody] UpdateCourierDto dto)
    {
        var res = await courierService.UpdateCourierAsync(id, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCourier(int id)
    {
        var res = await courierService.DeleteCourierAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> GetById(int id)
    {
        var res = await courierService.GetByIdAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var res = await courierService.GetAllAsync();
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("location")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateCourierLocationDto dto)
    {
        var res = await courierService.UpdateLocationAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("status")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> ChangeStatus([FromBody] UpdateCourierStatusDto dto)
    {
        var res = await courierService.ChangeStatusAsync(UserId, dto);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("available")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> GetAvailableCouriers()
    {
        var res = await courierService.GetAvailableCouriersAsync();
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpPut("{courierId}/assign/{orderId}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> AssignCourierToOrder(int courierId, int orderId)
    {
        var res = await courierService.AssignCourierToOrderAsync(orderId, courierId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("{id:int}/location")]
    [Authorize]
    public async Task<IActionResult> GetLocation(int id)
    {
        var res = await courierService.GetLocationAsync(id);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("map")]
    [Authorize]
    public async Task<IActionResult> GetMap()
    {
        var res = await courierService.GetMapAsync();
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("my-orders")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetMyOrders()
    {
        var res = await courierService.GetMyOrdersAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }


    [HttpGet("history")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetHistory([FromQuery] CourierHistoryFilter filter)
    {
        var res = await courierService.GetHistoryAsync(UserId, filter);
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPost("auto-assign/{orderId}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> AutoAssignCourierToOrder(int orderId)
    {
        var res = await courierService.AutoAssignCourierToOrderAsync(orderId);
        return StatusCode((int)res.StatusCode, res);
    }   
}