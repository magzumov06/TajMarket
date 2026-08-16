namespace Application.Features.Order.Dtos;

public record CreateReturnRequestDto(int OrderId, string Reason);