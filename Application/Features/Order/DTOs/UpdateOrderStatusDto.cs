using Domain.Enums;

namespace Application.Features.Order.DTOs;

public record UpdateOrderStatusDto(
    OrderStatus Status);
