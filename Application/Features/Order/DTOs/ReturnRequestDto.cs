using Domain.Enums;

namespace Application.Features.Order.Dtos;

public record ReturnRequestDto(
    int Id,
    int OrderId,
    string OrderNumber,
    string Reason,
    ReturnStatus Status,
    string? AdminComment,
    DateTime RequestedAt,
    DateTime? ResolvedAt
);