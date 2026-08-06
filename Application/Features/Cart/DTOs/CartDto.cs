namespace Application.Features.Cart.DTOs;

public record CartDto(
    int Id,
    List<CartItemDto> Items, 
    decimal TotalAmount);
