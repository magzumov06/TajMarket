namespace Domain.DTOs.OrderDto;

public record OrderPreviewDto(
    List<OrderPreviewItemDto> Items,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal ShippingCost,
    decimal TotalAmount
);