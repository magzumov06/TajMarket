using Application.Common.Interfaces;
using Application.Features.Notification.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Notification.Queries.GetAllNotifications;

public class GetAllNotificationsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetAllNotificationsQueryHandler> logger)
    : IRequestHandler<GetAllNotificationsQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Retrieving all notifications for user {UserId}", userId);

            var notifications = await context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {NotificationCount} notifications for user {UserId}", notifications.Count, userId);

            return notifications
                .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt))
                .ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving notifications for user {UserId}", userId);
            return new List<NotificationDto>();
        }
    }
}