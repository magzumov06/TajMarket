// Application/Features/Address/AddressMapper.cs
using Application.Features.Address.Dtos;
using Application.Features.Address.DTOs;

namespace Application.Features.Address;

internal static class AddressMapper
{
    public static AddressDto ToDto(Domain.Entities.AddressEntity.Address a) => new(
        a.Id,
        a.FullName,
        a.PhoneNumber,
        a.Country,
        a.City,
        a.Street,
        a.PostalCode,
        a.IsDefault,
        a.Latitude,
        a.Longitude
    );
}