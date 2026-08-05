using Application.Features.Address.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Address.Queries.GetAddressById;

public class GetAddressByIdQuery(int userId, int addressId) : IRequest<Response<AddressDto>>
{
    public int UserId { get; } = userId;
    public int AddressId { get; } = addressId;
}