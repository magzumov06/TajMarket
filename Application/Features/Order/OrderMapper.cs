using Application.Features.Address.DTOs;
using Application.Features.Order.DTOs;
using Application.Features.Payment.DTOs;
using Domain.Enums;

namespace Application.Features.Order;

internal static class OrderMapper
{
    public static OrderListDto ToListDto(Domain.Entities.OrderEntity.Order o) => new(
        o.Id,
        o.OrderNumber,
        o.OrderDate,
        o.Status,
        o.TotalAmount,
        o.OrderItems.Count
    );

    public static OrderDetailDto ToDetailDto(Domain.Entities.OrderEntity.Order o) => new(
        o.Id,
        o.OrderNumber,
        o.OrderDate,
        o.Status,
        o.SubTotal,
        o.ShippingCost,
        o.DiscountAmount,
        o.TotalAmount,

        new AddressDto(
            o.ShippingAddress.Id,
            o.ShippingAddress.FullName,
            o.ShippingAddress.PhoneNumber,
            o.ShippingAddress.Country,
            o.ShippingAddress.City,
            o.ShippingAddress.Street,
            o.ShippingAddress.PostalCode,
            o.ShippingAddress.IsDefault,
            o.ShippingAddress.Latitude,
            o.ShippingAddress.Longitude),

        o.OrderItems.Select(oi => new OrderItemDto(
            oi.ProductId,
            oi.ProductName,
            oi.ProductImageUrl,
            oi.Quantity,
            oi.UnitPrice,
            oi.TotalPrice)).ToList(),

        o.Payment == null
            ? null
            : new PaymentDto(
                o.Payment.Id,
                o.Payment.Amount,
                o.Payment.Method,
                o.Payment.Status,
                o.Payment.PaidAt),

        o.Courier == null
            ? null
            : new CourierInfoDto(
                o.Courier.Id,
                o.Courier.User.FullName,
                o.Courier.User.PhoneNumber,
                o.Courier.Status),

        o.CustomerConfirmedAt,
        o.Status is OrderStatus.Shipped or OrderStatus.Confirmed or OrderStatus.Processing
            ? o.DeliveryConfirmationCode
            : null
    );

    public static string StatusLabel(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Дар интизорӣ",
        OrderStatus.Confirmed => "Тасдиқшуда",
        OrderStatus.Processing => "Дар ҳоли тайёркунӣ",
        OrderStatus.Shipped => "Фиристода шуд",
        OrderStatus.Delivered => "Расонида шуд",
        OrderStatus.Cancelled => "Бекоршуда",
        OrderStatus.Returned => "Баргардонидашуда",
        _ => status.ToString()
    };
}