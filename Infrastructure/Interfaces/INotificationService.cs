using Domain.DTOs.NotificationDto;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public interface INotificationService
{
    Task<Responce<List<NotificationDto>>> GetAllAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<Responce<string>> MarkAsReadAsync(int userId, int notificationId);
    Task<Responce<string>> MarkAllAsReadAsync(int userId);

    Task NotifyAsync(int userId, string title, string message);
}