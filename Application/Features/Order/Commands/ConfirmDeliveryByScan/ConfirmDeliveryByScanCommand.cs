using Application.Features.Order.Dtos;
using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.ConfirmDeliveryByScan;

public class ConfirmDeliveryByScanCommand(int courierUserId, ConfirmDeliveryByScanDto dto) : IRequest<Response<OrderDetailDto>>
{
    public int CourierUserId { get; } = courierUserId;
    public ConfirmDeliveryByScanDto Dto { get; } = dto;
}