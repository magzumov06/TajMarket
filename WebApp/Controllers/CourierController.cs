using Application.Features.Courier.Commands.AssignCourierToOrder;
using Application.Features.Courier.Commands.AutoAssignCourierToOrder;
using Application.Features.Courier.Commands.ChangeCourierStatus;
using Application.Features.Courier.Commands.CreateCourier;
using Application.Features.Courier.Commands.DeleteCourier;
using Application.Features.Courier.Commands.UpdateCourier;
using Application.Features.Courier.Commands.UpdateCourierLocation;
using Application.Features.Courier.Queries.GetAllCouriers;
using Application.Features.Courier.Queries.GetAvailableCouriers;
using Application.Features.Courier.Queries.GetCourierById;
using Application.Features.Courier.Queries.GetCourierHistory;
using Application.Features.Courier.Queries.GetCourierLocation;
using Application.Features.Courier.Queries.GetCourierMap;
using Application.Features.Courier.Queries.GetMyOrders;
using Domain.DTOs.CourierDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class CouriersController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCourier([FromBody] CreateCourierDto dto)
    {
        var res = await mediator.Send(new CreateCourierCommand(dto));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPost("auto-assign/{orderId}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> AutoAssignCourierToOrder(int orderId)
    {
        var res = await mediator.Send(new AutoAssignCourierToOrderCommand(orderId));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPut("location")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateCourierLocationDto dto)
    {
        var res = await mediator.Send(new UpdateCourierLocationCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCourier(int id, [FromBody] UpdateCourierDto dto)
    {
        var res = await mediator.Send(new UpdateCourierCommand(id, dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPut("status")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> ChangeStatus([FromBody] UpdateCourierStatusDto dto)
    {
        var res = await mediator.Send(new ChangeCourierStatusCommand(UserId, dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPut("{courierId}/assign/{orderId}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> AssignCourierToOrder(int courierId, int orderId)
    {
        var res = await mediator.Send(new AssignCourierToOrderCommand(orderId, courierId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCourier(int id)
    {
        var res = await mediator.Send(new DeleteCourierCommand(id));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> GetById(int id)
    {
        var res = await mediator.Send(new GetCourierByIdQuery(id));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var res = await mediator.Send(new GetAllCouriersQuery());
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("available")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> GetAvailableCouriers()
    {
        var res = await mediator.Send(new GetAvailableCouriersQuery());
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpGet("{id:int}/location")]
    [Authorize]
    public async Task<IActionResult> GetLocation(int id)
    {
        var res = await mediator.Send(new GetCourierLocationQuery(id));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("map")]
    [Authorize]
    public async Task<IActionResult> GetMap()
    {
        var res = await mediator.Send(new GetCourierMapQuery());
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("my-orders")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetMyOrders()
    {
        var res = await mediator.Send(new GetMyOrdersQuery(UserId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("history")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetHistory([FromQuery] Application.Features.Courier.CourierHistoryFilter filter)
    {
        var res = await mediator.Send(new GetCourierHistoryQuery(UserId, filter));
        return StatusCode((int)res.StatusCode, res);
    }

}