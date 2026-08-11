using Application.Features.Notification.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Notification.Commands.BroadcastNotification;

public class BroadcastNotificationCommand(BroadcastNotificationDto dto) : IRequest<Response<string>>
{
    public BroadcastNotificationDto Dto { get; } = dto;
}