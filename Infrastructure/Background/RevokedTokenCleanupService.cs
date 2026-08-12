using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background;

public class RevokedTokenCleanupService(
    DataContext context,
    ILogger<RevokedTokenCleanupService> logger) : IRevokedTokenCleanupService
{
    public async Task DeleteExpiredTokensAsync()
    {
        try
        {
            logger.LogInformation("Starting cleanup of expired revoked tokens");

            var expired = await context.RevokedTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (expired.Count == 0)
            {
                logger.LogInformation("No expired revoked tokens to clean up");
                return;
            }

            context.RevokedTokens.RemoveRange(expired);
            await context.SaveChangesAsync();

            logger.LogInformation("Deleted {Count} expired revoked tokens", expired.Count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error cleaning up expired revoked tokens");
        }
    }
}