using MediatR;

namespace Application.Features.Notification.Queries.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQuery(int userId) : IRequest<int>
{
    public int UserId { get; } = userId;
}