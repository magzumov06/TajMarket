using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand(int sellerUserId, int orderId, UpdateOrderStatusDto dto)
    : IRequest<Response<OrderDetailDto>>
{
    public int SellerUserId { get; } = sellerUserId;
    public int OrderId { get; } = orderId;
    public UpdateOrderStatusDto Dto { get; } = dto;
}