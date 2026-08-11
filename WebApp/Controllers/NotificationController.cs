using Application.Features.Notification.Commands.BroadcastNotification;
using Application.Features.Notification.Commands.MarkAllNotificationsAsRead;
using Application.Features.Notification.Commands.MarkNotificationAsRead;
using Application.Features.Notification.Commands.SendNotification;
using Application.Features.Notification.Dtos;
using Application.Features.Notification.Queries.GetAllNotifications;
using Application.Features.Notification.Queries.GetUnreadNotificationCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
public class NotificationController(IMediator mediator) : BaseApiController
{
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendToUser([FromBody] SendNotificationToUserDto dto)
    {
        var res = await mediator.Send(new SendNotificationCommand(dto.UserId, dto.Title, dto.Message));
        return StatusCode((int)res.StatusCode, res);
    }
    
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationDto dto)
    {
        var res = await mediator.Send(new BroadcastNotificationCommand(dto));
        return StatusCode((int)res.StatusCode, res);
    }
    
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