using System.Net;
using Domain.DTOs.NotificationDto;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class NotificationService(DataContext context) : INotificationService
{
    public async Task<Response<string>> MarkAsReadAsync(int userId, int notificationId)
    {
        try
        {
            Log.Information("Marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
            var  notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
            if (notification == null)
                return new Response<string>(HttpStatusCode.NotFound,"Огоҳинома ёфт нашуд");
            
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await context.SaveChangesAsync();
            }
            Log.Information("Notification {NotificationId} marked as read", notificationId);
            return new Response<string>(HttpStatusCode.OK,"Notification has been read");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error marking notification {NotificationId} as read", notificationId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }

    public async Task<Response<string>> MarkAllAsReadAsync(int userId)
    {
        try
        {
            Log.Information("Marking all notifications as read for user {UserId}", userId);
            var unread = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            
            if (unread.Count == 0)
                return new Response<string>(HttpStatusCode.OK,"Notification has been read");
            
            foreach (var n in unread)
                n.IsRead = true;
            
            await context.SaveChangesAsync();
            Log.Information("Marked {NotificationCount} notifications as read for user {UserId}", unread.Count, userId);
            return new Response<string>(HttpStatusCode.OK,"Ҳамаи огоҳиномаҳо хонда шуданд");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error marking all notifications as read for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        Log.Information("Counting unread notifications for user {UserId}", userId);
        var count = await context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
        Log.Information("Unread notifications count for user {UserId}: {Count}", userId, count);
        return count;
    }

    public async Task<List<NotificationDto>> GetAllAsync(int userId)
    {
        Log.Information("Retrieving all notifications for user {UserId}", userId);
        var notifications = await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        
        Log.Information("Retrieved {NotificationCount} notifications for user {UserId}", notifications.Count, userId);
        return notifications
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt))
            .ToList();
    }

    public async Task NotifyAsync(int userId, string title, string message)
    {
        Log.Information("Sending notification to user {UserId}: {Title}", userId, title);
        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        Log.Information("Notification sent to user {UserId}", userId);
    }
}