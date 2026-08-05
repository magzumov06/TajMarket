using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Address.Commands.DeleteAddress;

public class DeleteAddressCommandHandler(
    IApplicationDbContext context,
    ILogger<DeleteAddressCommandHandler> logger)
    : IRequestHandler<DeleteAddressCommand, Response<string>>
{
    public async Task<Response<string>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var (userId, addressId) = (request.UserId, request.AddressId);

        try
        {
            logger.LogInformation("Deleting address {AddressId} for user {UserId}", addressId, userId);

            var address = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

            if (address == null)
            {
                logger.LogWarning("Address not found {AddressId} for user {UserId}", addressId, userId);
                return new Response<string>(HttpStatusCode.NotFound, "Суроға ёфт нашуд");
            }

            var usedInOrder = await context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId, cancellationToken);

            if (usedInOrder)
            {
                logger.LogWarning("Address {AddressId} cannot be deleted because it is used in orders", addressId);
                return new Response<string>(HttpStatusCode.BadRequest, "Ин суроға дар фармоишҳо истифода шудааст ва нест карда намешавад");
            }

            var wasDefault = address.IsDefault;

            context.Addresses.Remove(address);
            await context.SaveChangesAsync(cancellationToken);

            if (wasDefault)
            {
                var next = await context.Addresses
                    .Where(a => a.UserId == userId)
                    .OrderBy(a => a.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (next != null)
                {
                    next.IsDefault = true;
                    await context.SaveChangesAsync(cancellationToken);
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
}