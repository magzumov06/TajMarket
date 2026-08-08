using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Notification.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler(
    IApplicationDbContext context,
    ILogger<MarkNotificationAsReadCommandHandler> logger)
    : IRequestHandler<MarkNotificationAsReadCommand, Response<string>>
{
    public async Task<Response<string>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var (userId, notificationId) = (request.UserId, request.NotificationId);

        try
        {
            logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", notificationId, userId);

            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

            if (notification == null)
            {
                logger.LogWarning("Notification {NotificationId} not found for user {UserId}", notificationId, userId);
                return new Response<string>(HttpStatusCode.NotFound, "Огоҳинома ёфт нашуд");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Notification {NotificationId} marked as read", notificationId);

            return new Response<string>(HttpStatusCode.OK, "Notification has been read");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error marking notification {NotificationId} as read", notificationId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}