using Application.Features.Address.Commands.CreateAddress;
using Application.Features.Address.Commands.DeleteAddress;
using Application.Features.Address.Commands.SetDefaultAddress;
using Application.Features.Address.Commands.UpdateAddress;
using Application.Features.Address.Dtos;
using Application.Features.Address.Queries.GetAddressById;
using Application.Features.Address.Queries.GetAllAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
public class AddressesController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var result = await mediator.Send(new CreateAddressCommand(UserId, dto));
        return StatusCode((int)result.StatusCode, result);
    }
    
    [HttpPost("{addressId:int}/set-default")]
    public async Task<IActionResult> SetDefault(int addressId)
    {
        var result = await mediator.Send(new SetDefaultAddressCommand(UserId, addressId));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPut("{addressId:int}")]
    public async Task<IActionResult> Update(int addressId, [FromBody] CreateAddressDto dto)
    {
        var result = await mediator.Send(new UpdateAddressCommand(UserId, addressId, dto));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("{addressId:int}")]
    public async Task<IActionResult> Delete(int addressId)
    {
        var result = await mediator.Send(new DeleteAddressCommand(UserId, addressId));
        return StatusCode((int)result.StatusCode, result);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new GetAllAddressesQuery(UserId));
        return Ok(result);
    }

    [HttpGet("{addressId:int}")]
    public async Task<IActionResult> GetById(int addressId)
    {
        var result = await mediator.Send(new GetAddressByIdQuery(UserId, addressId));
        return StatusCode((int)result.StatusCode, result);
    }
}