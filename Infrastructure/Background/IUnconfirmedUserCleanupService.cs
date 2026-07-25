namespace Infrastructure.Background;

public interface IUnconfirmedUserCleanupService
{
    Task DeleteOldUnconfirmedUsersAsync();
}