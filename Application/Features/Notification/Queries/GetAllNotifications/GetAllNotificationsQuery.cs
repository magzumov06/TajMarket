using Application.Features.Notification.DTOs;
using MediatR;

namespace Application.Features.Notification.Queries.GetAllNotifications;

public class GetAllNotificationsQuery(int userId) : IRequest<List<NotificationDto>>
{
    public int UserId { get; } = userId;
}