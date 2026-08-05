using Application.Features.Address.DTOs;
using MediatR;

namespace Application.Features.Address.Queries.GetAllAddresses;

public class GetAllAddressesQuery(int userId) : IRequest<List<AddressDto>>
{
    public int UserId { get; } = userId;
}