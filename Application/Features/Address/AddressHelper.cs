using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Address;

internal static class AddressHelper
{
    public static async Task UnsetPreviousDefaultAsync(IApplicationDbContext context, int userId, ILogger logger)
    {
        var current = await context.Addresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync();

        foreach (var a in current)
            a.IsDefault = false;

        if (current.Count > 0)
            logger.LogInformation("Removed previous default addresses for user {UserId}", userId);
    }
}