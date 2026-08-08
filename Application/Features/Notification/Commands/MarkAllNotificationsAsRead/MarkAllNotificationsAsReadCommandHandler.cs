using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Notification.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler(
    IApplicationDbContext context,
    ILogger<MarkAllNotificationsAsReadCommandHandler> logger)
    : IRequestHandler<MarkAllNotificationsAsReadCommand, Response<string>>
{
    public async Task<Response<string>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Marking all notifications as read for user {UserId}", userId);

            var unread = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            if (unread.Count == 0)
            {
                logger.LogInformation("No unread notifications found for user {UserId}", userId);
                return new Response<string>(HttpStatusCode.OK, "Notification has been read");
            }

            foreach (var n in unread)
                n.IsRead = true;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Marked {NotificationCount} notifications as read for user {UserId}", unread.Count, userId);

            return new Response<string>(HttpStatusCode.OK, "Ҳамаи огоҳиномаҳо хонда шуданд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error marking all notifications as read for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }
}