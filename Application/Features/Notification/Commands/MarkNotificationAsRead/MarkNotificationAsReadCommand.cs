using Domain.Responses;
using MediatR;

namespace Application.Features.Notification.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommand(int userId, int notificationId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int NotificationId { get; } = notificationId;
}