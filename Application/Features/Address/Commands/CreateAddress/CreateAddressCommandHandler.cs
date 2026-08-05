using System.Net;
using Application.Common.Interfaces;
using Application.Features.Address.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Address.Commands.CreateAddress;

public class CreateAddressCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateAddressCommandHandler> logger)
    : IRequestHandler<CreateAddressCommand, Response<AddressDto>>
{
    public async Task<Response<AddressDto>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("Creating address for user {UserId}", userId);

            var isFirstAddress = !await context.Addresses.AnyAsync(a => a.UserId == userId, cancellationToken);

            var address = new Domain.Entities.AddressEntity.Address
            {
                UserId = userId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Country = dto.Country,
                City = dto.City,
                Street = dto.Street,
                PostalCode = dto.PostalCode,
                IsDefault = isFirstAddress || dto.IsDefault,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            if (address.IsDefault)
                await AddressHelper.UnsetPreviousDefaultAsync(context, userId, logger);

            context.Addresses.Add(address);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Address created successfully {AddressId} for user {UserId}", address.Id, userId);

            return new Response<AddressDto>(AddressMapper.ToDto(address), "Суроға илова шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating address for user {UserId}", userId);
            return new Response<AddressDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}