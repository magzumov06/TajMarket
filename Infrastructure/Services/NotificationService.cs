using System.Net;
using Domain.DTOs.NotificationDto;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class NotificationService(DataContext context) : INotificationService
{
    public async Task<Response<string>> MarkAsReadAsync(int userId, int notificationId)
    {
        try
        {
            var  notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
            if (notification == null)
                return new Response<string>(HttpStatusCode.NotFound,"Огоҳинома ёфт нашуд");
            
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await context.SaveChangesAsync();
            }
            return new Response<string>(HttpStatusCode.OK,"Notification has been read");
        }
        catch (Exception e)
        {
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }

    public async Task<Response<string>> MarkAllAsReadAsync(int userId)
    {
        try
        {
            var unread = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            
            if (unread.Count == 0)
                return new Response<string>(HttpStatusCode.OK,"Notification has been read");
            
            foreach (var n in unread)
                n.IsRead = true;
            
            await context.SaveChangesAsync();
            return new Response<string>(HttpStatusCode.OK,"Ҳамаи огоҳиномаҳо хонда шуданд");
        }
        catch (Exception e)
        {
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        try
        {
            return await context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<List<NotificationDto>> GetAllAsync(int userId)
    {
        try
        {
            var notifications = await context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            
            return notifications
                .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt))
                .ToList();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task NotifyAsync(int userId, string title, string message)
    {
        try
        {
            context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw;
        }
    }
}