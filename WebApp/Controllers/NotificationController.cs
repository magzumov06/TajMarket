using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var res = await notificationService.MarkAsReadAsync(UserId, notificationId);
        return StatusCode((int)res.StatusCode, res);
    }

    
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var res = await notificationService.MarkAllAsReadAsync(UserId);
        return StatusCode((int)res.StatusCode, res);
    }

    
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await notificationService.GetUnreadCountAsync(UserId);
        return Ok(new { unreadCount = count });
    }

    
    [HttpGet]
    public async Task<IActionResult> GetAllNotifications()
    {
        var notifications = await notificationService.GetAllAsync(UserId);
        return Ok(notifications);
    }
}
