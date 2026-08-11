namespace Application.Features.Notification.Dtos;

public record SendNotificationToUserDto(int UserId, string Title, string Message);