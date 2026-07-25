using Domain.Entities.UserEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background;

public class UnconfirmedUserCleanupService(
    UserManager<User> userManager,
    ILogger<UnconfirmedUserCleanupService> logger) : IUnconfirmedUserCleanupService
{
    public async Task DeleteOldUnconfirmedUsersAsync()
    {
        try
        {
            var expirationDate = DateTime.UtcNow.AddHours(-24);

            var users = await userManager.Users
                .Where(u =>
                    !u.EmailConfirmed &&
                    u.CreatedAt <= expirationDate)
                .ToListAsync();

            if (users.Count == 0)
            {
                logger.LogInformation("No unconfirmed users older than 24 hours found");
                return;
            }

            foreach (var user in users)
            {
                var result = await userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    logger.LogInformation(
                        "Deleted unconfirmed user {UserId} {Email}",
                        user.Id,
                        user.Email);
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));

                    logger.LogWarning(
                        "Failed to delete unconfirmed user {UserId}. Errors: {Errors}",
                        user.Id,
                        errors);
                }
            }

            logger.LogInformation(
                "Unconfirmed user cleanup completed. Deleted/checked count: {Count}",
                users.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unconfirmed user cleanup failed");
        }
    }
}