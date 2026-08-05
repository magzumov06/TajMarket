using MediatR;

namespace Application.Features.Notification.Commands.SendNotification;

public class SendNotificationCommand(int userId, string title, string message) : IRequest
{
    public int UserId { get; } = userId;
    public string Title { get; } = title;
    public string Message { get; } = message;
}