using System.Net;
using Domain.DTOs.AddressDtos;
using Domain.Entities.AddressEntity;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AddressService(
    DataContext context,
    ILogger<AddressService> logger) : IAddressService
{
    public async Task<List<AddressDto>> GetAllAsync(int userId)
    {
        try
        {
            logger.LogInformation("Getting all addresses for user {UserId}", userId);
            
            var addresses = await context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();


            logger.LogInformation("Retrieved {AddressCount} addresses for user {UserId}",addresses.Count,userId);
            
            return addresses.Select(ToDto).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting addresses for user {UserId}", userId);
            throw;
        }
    }


    public async Task<Response<AddressDto>> GetByIdAsync(int userId, int addressId)
    {
        try
        {
            logger.LogInformation("Getting address {AddressId} for user {UserId}", addressId, userId);
            
            var address = await context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    a =>
                        a.Id == addressId &&
                        a.UserId == userId);
            
            if (address == null)
            {
                logger.LogWarning("Address not found {AddressId} for user {UserId}", addressId, userId);

                return new Response<AddressDto>(HttpStatusCode.NotFound, "Суроға ёфт нашуд");
            }
            
            return new Response<AddressDto>(ToDto(address));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting address {AddressId} for user {UserId}", addressId, userId);

            return new Response<AddressDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<AddressDto>> CreateAsync(int userId, CreateAddressDto dto)
    {
        try
        {
            logger.LogInformation("Creating address for user {UserId}", userId);
            
            var isFirstAddress = !await context.Addresses
                .AnyAsync(a => a.UserId == userId);


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


            logger.LogInformation("Address created successfully {AddressId} for user {UserId}", address.Id, userId);


            return new Response<AddressDto>(ToDto(address), "Суроға илова шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating address for user {UserId}", userId);

            return new Response<AddressDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<AddressDto>> UpdateAsync(int userId, int addressId, CreateAddressDto dto)
    {
        try
        {
            logger.LogInformation("Updating address {AddressId} for user {UserId}", addressId, userId);
            
            var address = await context.Addresses
                .FirstOrDefaultAsync(
                    a =>
                        a.Id == addressId &&
                        a.UserId == userId);


            if (address == null)
            {
                logger.LogWarning("Address not found {AddressId} for user {UserId}", addressId, userId);

                return new Response<AddressDto>(HttpStatusCode.NotFound, "Суроға ёфт нашуд");
            }
            
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


            logger.LogInformation("Address updated successfully {AddressId}", addressId);


            return new Response<AddressDto>(ToDto(address), "Суроға навсозӣ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating address {AddressId} for user {UserId}", addressId, userId);

            return new Response<AddressDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
       public async Task<Response<string>> DeleteAsync(int userId, int addressId)
    {
        try
        {
            logger.LogInformation("Deleting address {AddressId} for user {UserId}", addressId, userId);

            var address = await context.Addresses
                .FirstOrDefaultAsync(
                    a =>
                        a.Id == addressId &&
                        a.UserId == userId);


            if (address == null)
            {
                logger.LogWarning("Address not found {AddressId} for user {UserId}", addressId, userId);

                return new Response<string>(HttpStatusCode.NotFound, "Суроға ёфт нашуд");
            }


            var usedInOrder = await context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId);


            if (usedInOrder)
            {
                logger.LogWarning("Address {AddressId} cannot be deleted because it is used in orders", addressId);

                return new Response<string>(HttpStatusCode.BadRequest, "Ин суроға дар фармоишҳо истифода шудааст ва нест карда намешавад");
            }

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
                    
                    logger.LogInformation("New default address set {AddressId} for user {UserId}", next.Id, userId);
                }
            }
            
            logger.LogInformation("Address deleted successfully {AddressId} for user {UserId}", addressId, userId);

            return new Response<string>(HttpStatusCode.OK, "Суроға нест шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting address {AddressId} for user {UserId}", addressId, userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    public async Task<Response<string>> SetDefaultAsync(int userId, int addressId)
    {
        try
        {
            logger.LogInformation("Setting default address {AddressId} for user {UserId}", addressId, userId);

            var address = await context.Addresses
                .FirstOrDefaultAsync(
                    a =>
                        a.Id == addressId &&
                        a.UserId == userId);


            if (address == null)
            {
                logger.LogWarning("Address not found {AddressId} for user {UserId}", addressId, userId);

                return new Response<string>(HttpStatusCode.NotFound, "Суроға ёфт нашуд");
            }
            
            await UnsetPreviousDefaultAsync(userId);

            address.IsDefault = true;
            
            await context.SaveChangesAsync();
            
            logger.LogInformation("Default address changed successfully {AddressId} for user {UserId}", addressId, userId);

            return new Response<string>(HttpStatusCode.OK, "Суроғаи асосӣ иваз шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error setting default address {AddressId} for user {UserId}", addressId, userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }


    private async Task UnsetPreviousDefaultAsync(int userId)
    {
        var current = await context.Addresses
            .Where(a =>
                a.UserId == userId &&
                a.IsDefault)
            .ToListAsync();


        foreach (var a in current)
        {
            a.IsDefault = false;
        }


        if (current.Count > 0)
        {
            logger.LogInformation("Removed previous default addresses for user {UserId}", userId);
        }
    }


    private static AddressDto ToDto(Address a) => new(
        a.Id,
        a.FullName,
        a.PhoneNumber,
        a.Country,
        a.City,
        a.Street,
        a.PostalCode,
        a.IsDefault
    );
}