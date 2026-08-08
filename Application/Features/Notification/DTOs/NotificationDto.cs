namespace Application.Features.Notification.DTOs;

public record NotificationDto(
    int Id, 
    string Title, 
    string Message,
    bool IsRead, 
    DateTime CreatedAt
    );
