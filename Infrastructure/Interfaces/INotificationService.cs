using Domain.DTOs.NotificationDto;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface INotificationService
{
    Task<Response<string>> MarkAsReadAsync(int userId, int notificationId);
    Task<Response<string>> MarkAllAsReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<List<NotificationDto>> GetAllAsync(int userId);

    Task NotifyAsync(int userId, string title, string message);
}