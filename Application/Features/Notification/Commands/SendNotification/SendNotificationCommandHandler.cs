using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Notification.Commands.SendNotification;

public class SendNotificationCommandHandler(
    IApplicationDbContext context,
    IRealtimeNotifier realtimeNotifier,
    ISmsService smsService,  
    ILogger<SendNotificationCommandHandler> logger)
    : IRequestHandler<SendNotificationCommand, Response<string>>
{
    public async Task<Response<string>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Sending notification to user {UserId}: {Title}", request.UserId, request.Title);

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("Notification target user not found {UserId}", request.UserId);
                return new Response<string>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }

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

            await realtimeNotifier.NotifyUserAsync(request.UserId, new Application.Common.Interfaces.NotificationPayload(
                notification.Id, notification.Title, notification.Message, notification.IsRead, notification.CreatedAt));

            if (request.AlsoSms && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await smsService.SendSmsAsync(user.PhoneNumber, $"{request.Title}: {request.Message}");
            }

            logger.LogInformation("Notification sent successfully to user {UserId}", request.UserId);

            return new Response<string>(HttpStatusCode.OK, "Огоҳинома фиристода шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error sending notification to user {UserId}", request.UserId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Огоҳинома фиристода нашуд");
        }
    }
}