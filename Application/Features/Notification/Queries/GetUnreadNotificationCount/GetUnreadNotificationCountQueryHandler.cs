using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Notification.Queries.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandler(
    IApplicationDbContext context,
    ILogger<GetUnreadNotificationCountQueryHandler> logger)
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Counting unread notifications for user {UserId}", userId);

            var count = await context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

            logger.LogInformation("Unread notifications count for user {UserId}: {Count}", userId, count);

            return count;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error counting unread notifications for user {UserId}", userId);
            return 0;
        }
    }
}