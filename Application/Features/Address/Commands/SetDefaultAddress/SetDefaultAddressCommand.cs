using Domain.Responses;
using MediatR;

namespace Application.Features.Address.Commands.SetDefaultAddress;

public class SetDefaultAddressCommand(int userId, int addressId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int AddressId { get; } = addressId;
}