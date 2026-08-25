using Application.Features.Order.Dtos;
using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.ConfirmDeliveryByCode;

public class ConfirmDeliveryByCodeCommand(int courierUserId, int orderId, ConfirmDeliveryByCodeDto dto)
    : IRequest<Response<OrderDetailDto>>
{
    public int CourierUserId { get; } = courierUserId;
    public int OrderId { get; } = orderId;
    public ConfirmDeliveryByCodeDto Dto { get; } = dto;
}