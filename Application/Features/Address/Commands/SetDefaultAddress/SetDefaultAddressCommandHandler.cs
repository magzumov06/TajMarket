using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Address.Commands.SetDefaultAddress;

public class SetDefaultAddressCommandHandler(
    IApplicationDbContext context,
    ILogger<SetDefaultAddressCommandHandler> logger)
    : IRequestHandler<SetDefaultAddressCommand, Response<string>>
{
    public async Task<Response<string>> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var (userId, addressId) = (request.UserId, request.AddressId);

        try
        {
            logger.LogInformation("Setting default address {AddressId} for user {UserId}", addressId, userId);

            var address = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

            if (address == null)
            {
                logger.LogWarning("Address not found {AddressId} for user {UserId}", addressId, userId);
                return new Response<string>(HttpStatusCode.NotFound, "Суроға ёфт нашуд");
            }

            await AddressHelper.UnsetPreviousDefaultAsync(context, userId, logger);
            address.IsDefault = true;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Default address changed successfully {AddressId} for user {UserId}", addressId, userId);

            return new Response<string>(HttpStatusCode.OK, "Суроғаи асосӣ иваз шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error setting default address {AddressId} for user {UserId}", addressId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}