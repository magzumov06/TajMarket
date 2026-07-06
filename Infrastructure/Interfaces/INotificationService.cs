using Domain.DTOs.NotificationDto;
using Domain.Resposes;

namespace Infrastructure.Interfaces;

public interface INotificationService
{
    Task<Response<List<NotificationDto>>> GetAllAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<Response<string>> MarkAsReadAsync(int userId, int notificationId);
    Task<Response<string>> MarkAllAsReadAsync(int userId);

    Task NotifyAsync(int userId, string title, string message);
}