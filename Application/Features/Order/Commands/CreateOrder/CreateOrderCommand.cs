using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.CreateOrder;

public class CreateOrderCommand(int userId, CreateOrderDto dto) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public CreateOrderDto Dto { get; } = dto;
}