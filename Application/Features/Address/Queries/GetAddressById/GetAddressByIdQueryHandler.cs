using System.Net;
using Application.Common.Interfaces;
using Application.Features.Address.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Address.Queries.GetAddressById;

public class GetAddressByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetAddressByIdQueryHandler> logger)
    : IRequestHandler<GetAddressByIdQuery, Response<AddressDto>>
{
    public async Task<Response<AddressDto>> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var (userId, addressId) = (request.UserId, request.AddressId);

        try
        {
            logger.LogInformation("Getting address {AddressId} for user {UserId}", addressId, userId);

            var address = await context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

            if (address == null)
            {
                logger.LogWarning("Address not found {AddressId} for user {UserId}", addressId, userId);
                return new Response<AddressDto>(HttpStatusCode.NotFound, "Суроға ёфт нашуд");
            }

            return new Response<AddressDto>(AddressMapper.ToDto(address));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting address {AddressId} for user {UserId}", addressId, userId);
            return new Response<AddressDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}