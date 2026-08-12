namespace Infrastructure.Background;

public interface IRevokedTokenCleanupService
{
    Task DeleteExpiredTokensAsync();
}