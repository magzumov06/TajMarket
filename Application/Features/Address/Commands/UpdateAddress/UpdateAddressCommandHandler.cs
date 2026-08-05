using System.Net;
using Application.Common.Interfaces;
using Application.Features.Address.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Address.Commands.UpdateAddress;

public class UpdateAddressCommandHandler(
    IApplicationDbContext context,
    ILogger<UpdateAddressCommandHandler> logger)
    : IRequestHandler<UpdateAddressCommand, Response<AddressDto>>
{
    public async Task<Response<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var (userId, addressId, dto) = (request.UserId, request.AddressId, request.Dto);

        try
        {
            logger.LogInformation("Updating address {AddressId} for user {UserId}", addressId, userId);

            var address = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

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
            address.Latitude = dto.Latitude;
            address.Longitude = dto.Longitude;

            if (dto.IsDefault && !address.IsDefault)
            {
                await AddressHelper.UnsetPreviousDefaultAsync(context, userId, logger);
                address.IsDefault = true;
            }

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Address updated successfully {AddressId}", addressId);

            return new Response<AddressDto>(AddressMapper.ToDto(address), "Суроға навсозӣ шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating address {AddressId} for user {UserId}", addressId, userId);
            return new Response<AddressDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}