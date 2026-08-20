using Application.Features.Courier.DTOs;

namespace Application.Features.Courier;

internal static class CourierMapper
{
    public static CourierDto ToDto(Domain.Entities.UserEntity.Courier c, Domain.Entities.UserEntity.User user) => new(
        c.Id, c.UserId, user.FullName, user.PhoneNumber,
        c.Status, c.VehicleType, c.Latitude, c.Longitude, c.LastLocationUpdate, c.CreatedAt);

    public static CourierOrderDto ToOrderDto(Domain.Entities.OrderEntity.Order o) => new(
        o.Id, o.OrderNumber, o.OrderDate, o.Status, o.TotalAmount,
        o.ShippingAddress.City, o.ShippingAddress.Street,
        o.Buyer.FullName, o.Buyer.PhoneNumber);
}