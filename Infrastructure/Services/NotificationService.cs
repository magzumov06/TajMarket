using System.Net;
using Domain.DTOs.NotificationDto;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class NotificationService(
    DataContext context,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<Response<string>> MarkAsReadAsync(int userId, int notificationId)
    {
        try
        {
            logger.LogInformation(
                "Marking notification {NotificationId} as read for user {UserId}",
                notificationId,
                userId);


            var notification = await context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.Id == notificationId &&
                    n.UserId == userId);


            if (notification == null)
            {
                logger.LogWarning(
                    "Notification {NotificationId} not found for user {UserId}",
                    notificationId,
                    userId);

                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "Огоҳинома ёфт нашуд");
            }


            if (!notification.IsRead)
            {
                notification.IsRead = true;

                await context.SaveChangesAsync();
            }


            logger.LogInformation(
                "Notification {NotificationId} marked as read",
                notificationId);


            return new Response<string>(
                HttpStatusCode.OK,
                "Notification has been read");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error marking notification {NotificationId} as read",
                notificationId);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal Server Error");
        }
    }


    public async Task<Response<string>> MarkAllAsReadAsync(int userId)
    {
        try
        {
            logger.LogInformation(
                "Marking all notifications as read for user {UserId}",
                userId);


            var unread = await context.Notifications
                .Where(n =>
                    n.UserId == userId &&
                    !n.IsRead)
                .ToListAsync();


            if (unread.Count == 0)
            {
                logger.LogInformation(
                    "No unread notifications found for user {UserId}",
                    userId);

                return new Response<string>(
                    HttpStatusCode.OK,
                    "Notification has been read");
            }


            foreach (var n in unread)
                n.IsRead = true;


            await context.SaveChangesAsync();


            logger.LogInformation(
                "Marked {NotificationCount} notifications as read for user {UserId}",
                unread.Count,
                userId);


            return new Response<string>(
                HttpStatusCode.OK,
                "Ҳамаи огоҳиномаҳо хонда шуданд");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error marking all notifications as read for user {UserId}",
                userId);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal Server Error");
        }
    }


    public async Task<int> GetUnreadCountAsync(int userId)
    {
        try
        {
            logger.LogInformation(
                "Counting unread notifications for user {UserId}",
                userId);


            var count = await context.Notifications
                .AsNoTracking()
                .CountAsync(n =>
                    n.UserId == userId &&
                    !n.IsRead);


            logger.LogInformation(
                "Unread notifications count for user {UserId}: {Count}",
                userId,
                count);


            return count;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error counting unread notifications for user {UserId}",
                userId);

            return 0;
        }
    }


    public async Task<List<NotificationDto>> GetAllAsync(int userId)
    {
        try
        {
            logger.LogInformation(
                "Retrieving all notifications for user {UserId}",
                userId);


            var notifications = await context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();


            logger.LogInformation(
                "Retrieved {NotificationCount} notifications for user {UserId}",
                notifications.Count,
                userId);


            return notifications
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt))
                .ToList();
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error retrieving notifications for user {UserId}",
                userId);

            return new List<NotificationDto>();
        }
    }


    public async Task NotifyAsync(int userId, string title, string message)
    {
        try
        {
            logger.LogInformation(
                "Sending notification to user {UserId}: {Title}",
                userId,
                title);


            context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });


            await context.SaveChangesAsync();


            logger.LogInformation(
                "Notification sent successfully to user {UserId}",
                userId);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error sending notification to user {UserId}",
                userId);
        }
    }
}