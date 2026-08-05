using Application.Common.Interfaces;
using Application.Features.Address.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Address.Queries.GetAllAddresses;

public class GetAllAddressesQueryHandler(
    IApplicationDbContext context,
    ILogger<GetAllAddressesQueryHandler> logger)
    : IRequestHandler<GetAllAddressesQuery, List<AddressDto>>
{
    public async Task<List<AddressDto>> Handle(GetAllAddressesQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Getting all addresses for user {UserId}", userId);

            var addresses = await context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {AddressCount} addresses for user {UserId}", addresses.Count, userId);

            return addresses.Select(AddressMapper.ToDto).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting addresses for user {UserId}", userId);
            throw;
        }
    }
}