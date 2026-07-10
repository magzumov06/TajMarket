using Domain.DTOs.AddressDtos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
public class AddressesController(IAddressService addressService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await addressService.GetAllAsync(UserId);
        return Ok(result);
    }

    [HttpGet("{addressId:int}")]
    public async Task<IActionResult> GetById(int addressId)
    {
        var result = await addressService.GetByIdAsync(UserId, addressId);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var result = await addressService.CreateAsync(UserId, dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPut("{addressId:int}")]
    public async Task<IActionResult> Update(int addressId, [FromBody] CreateAddressDto dto)
    {
        var result = await addressService.UpdateAsync(UserId, addressId, dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("{addressId:int}")]
    public async Task<IActionResult> Delete(int addressId)
    {
        var result = await addressService.DeleteAsync(UserId, addressId);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("{addressId:int}/set-default")]
    public async Task<IActionResult> SetDefault(int addressId)
    {
        var result = await addressService.SetDefaultAsync(UserId, addressId);
        return StatusCode((int)result.StatusCode, result);
    }
}