using System.Net;
using Domain.DTOs.AddressDtos;
using Domain.Entities.AddressEntity;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AddressService(DataContext context) : IAddressService
{
    public async Task<List<AddressDto>> GetAllAsync(int userId)
    {
        var addresses = await context.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();

        return addresses.Select(ToDto).ToList();
    }

    public async Task<Response<AddressDto>> GetByIdAsync(int userId, int addressId)
    {
        var address = await context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        return address == null
            ? new Response<AddressDto>(HttpStatusCode.NotFound,"Суроға ёфт нашуд")
            : new Response<AddressDto>(ToDto(address));
    }

    public async Task<Response<AddressDto>> CreateAsync(int userId, CreateAddressDto dto)
    {
        var isFirstAddress = !await context.Addresses.AnyAsync(a => a.UserId == userId);

        var address = new Address
        {
            UserId = userId,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Country = dto.Country,
            City = dto.City,
            Street = dto.Street,
            PostalCode = dto.PostalCode,
            IsDefault = isFirstAddress || dto.IsDefault
        };

        if (address.IsDefault)
            await UnsetPreviousDefaultAsync(userId);

        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        return new Response<AddressDto>(ToDto(address), "Суроға илова шуд");
    }

    public async Task<Response<AddressDto>> UpdateAsync(int userId, int addressId, CreateAddressDto dto)
    {
        var address = await context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        if (address == null)
            return new Response<AddressDto>(HttpStatusCode.NotFound,"Суроға ёфт нашуд");

        address.FullName = dto.FullName;
        address.PhoneNumber = dto.PhoneNumber;
        address.Country = dto.Country;
        address.City = dto.City;
        address.Street = dto.Street;
        address.PostalCode = dto.PostalCode;

        if (dto.IsDefault && !address.IsDefault)
        {
            await UnsetPreviousDefaultAsync(userId);
            address.IsDefault = true;
        }

        await context.SaveChangesAsync();
        return new Response<AddressDto>(ToDto(address), "Суроға навсозӣ шуд");
    }

    public async Task<Response<string>> DeleteAsync(int userId, int addressId)
    {
        var address = await context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        if (address == null)
            return new Response<string>(HttpStatusCode.NotFound,"Суроға ёфт нашуд");

        var usedInOrder = await context.Orders.AnyAsync(o => o.ShippingAddressId == addressId);
        if (usedInOrder)
            return new Response<string>(HttpStatusCode.BadRequest,"Ин суроға дар фармоишҳо истифода шудааст ва нест карда намешавад");

        var wasDefault = address.IsDefault;
        context.Addresses.Remove(address);
        await context.SaveChangesAsync();

        if (wasDefault)
        {
            var next = await context.Addresses
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Id)
                .FirstOrDefaultAsync();

            if (next != null)
            {
                next.IsDefault = true;
                await context.SaveChangesAsync();
            }
        }

        return new Response<string>(HttpStatusCode.OK,"Суроға нест шуд");
    }

    public async Task<Response<string>> SetDefaultAsync(int userId, int addressId)
    {
        var address = await context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        if (address == null)
            return new Response<string>(HttpStatusCode.NotFound,"Суроға ёфт нашуд");

        await UnsetPreviousDefaultAsync(userId);
        address.IsDefault = true;
        await context.SaveChangesAsync();

        return new Response<string>(HttpStatusCode.OK,"Суроғаи асосӣ иваз шуд");
    }

    private async Task UnsetPreviousDefaultAsync(int userId)
    {
        var current = await context.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
        foreach (var a in current)
            a.IsDefault = false;
    }

    private static AddressDto ToDto(Address a) => new(
        a.Id, a.FullName, a.PhoneNumber, a.Country, a.City, a.Street, a.PostalCode, a.IsDefault
    );
}
