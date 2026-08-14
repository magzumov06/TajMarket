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
            logger.LogInformation("Starting cleanup of expired tokens");

            var expiredRevoked = await context.RevokedTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            var expiredRefresh = await context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            context.RevokedTokens.RemoveRange(expiredRevoked);
            context.RefreshTokens.RemoveRange(expiredRefresh);

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Deleted {RevokedCount} revoked tokens and {RefreshCount} expired refresh tokens",
                expiredRevoked.Count, expiredRefresh.Count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error cleaning up expired tokens");
        }
    }
}