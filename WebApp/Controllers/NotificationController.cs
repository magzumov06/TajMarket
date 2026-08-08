using Application.Features.Notification.Commands.MarkAllNotificationsAsRead;
using Application.Features.Notification.Commands.MarkNotificationAsRead;
using Application.Features.Notification.Queries.GetAllNotifications;
using Application.Features.Notification.Queries.GetUnreadNotificationCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
public class NotificationController(IMediator mediator) : BaseApiController
{
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var res = await mediator.Send(new MarkNotificationAsReadCommand(UserId, notificationId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var res = await mediator.Send(new MarkAllNotificationsAsReadCommand(UserId));
        return StatusCode((int)res.StatusCode, res);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await mediator.Send(new GetUnreadNotificationCountQuery(UserId));
        return Ok(new { unreadCount = count });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotifications()
    {
        var notifications = await mediator.Send(new GetAllNotificationsQuery(UserId));
        return Ok(notifications);
    }
}