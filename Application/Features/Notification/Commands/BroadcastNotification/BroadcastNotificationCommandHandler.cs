using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Notification.Commands.BroadcastNotification;

public class BroadcastNotificationCommandHandler(
    IApplicationDbContext context,
    IRealtimeNotifier realtimeNotifier,
    ILogger<BroadcastNotificationCommandHandler> logger)
    : IRequestHandler<BroadcastNotificationCommand, Response<string>>
{
    public async Task<Response<string>> Handle(BroadcastNotificationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        logger.LogInformation("Broadcasting notification to all users: {Title}", dto.Title);

        var userIds = await context.Users
            .Where(u => u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (userIds.Count == 0)
            return new Response<string>(HttpStatusCode.OK, "Ҳеҷ корбари фаъол ёфт нашуд");

        var now = DateTime.UtcNow;

        var notifications = userIds.Select(userId => new Domain.Entities.Notification
        {
            UserId = userId,
            Title = dto.Title,
            Message = dto.Message,
            IsRead = false,
            CreatedAt = now
        }).ToList();

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);
        
        await realtimeNotifier.BroadcastNotificationAsync(new NotificationPayload(
            0, dto.Title, dto.Message, false, now));

        logger.LogInformation("Notification broadcast to {UserCount} users", userIds.Count);

        return new Response<string>(HttpStatusCode.OK, $"Огоҳинома ба {userIds.Count} корбар фиристода шуд");
    }
}