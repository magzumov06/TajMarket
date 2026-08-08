using Domain.Responses;
using MediatR;

namespace Application.Features.Notification.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommand(int userId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
}