using Application.Features.Address.DTOs;
using Application.Features.Order.DTOs;
using Domain.DTOs.PaymentDtos;

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

        o.CustomerConfirmedAt
    );

    public static string StatusLabel(Domain.Enums.OrderStatus status) => status switch
    {
        Domain.Enums.OrderStatus.Pending => "Дар интизорӣ",
        Domain.Enums.OrderStatus.Confirmed => "Тасдиқшуда",
        Domain.Enums.OrderStatus.Processing => "Дар ҳоли тайёркунӣ",
        Domain.Enums.OrderStatus.Shipped => "Фиристода шуд",
        Domain.Enums.OrderStatus.Delivered => "Расонида шуд",
        Domain.Enums.OrderStatus.Cancelled => "Бекоршуда",
        Domain.Enums.OrderStatus.Returned => "Баргардонидашуда",
        _ => status.ToString()
    };
}