using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Notification.Commands.SendNotification;

public class SendNotificationCommandHandler(
    IApplicationDbContext context,
    IRealtimeNotifier realtimeNotifier,   
    ILogger<SendNotificationCommandHandler> logger)
    : IRequestHandler<SendNotificationCommand>
{
    public async Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Sending notification to user {UserId}: {Title}", request.UserId, request.Title);

            var notification = new Domain.Entities.Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync(cancellationToken);

            await realtimeNotifier.NotifyUserAsync(request.UserId, new NotificationPayload(
                notification.Id, notification.Title, notification.Message, notification.IsRead, notification.CreatedAt));

            logger.LogInformation("Notification sent successfully to user {UserId}", request.UserId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error sending notification to user {UserId}", request.UserId);
        }
    }
}