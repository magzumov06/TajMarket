namespace Domain.DTOs.CartDto;

public record CartDto(int Id, List<CartItemDto> Items, decimal TotalAmount);
