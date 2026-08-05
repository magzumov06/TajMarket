using Domain.Responses;
using MediatR;

namespace Application.Features.Address.Commands.DeleteAddress;

public class DeleteAddressCommand(int userId, int addressId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public int AddressId { get; } = addressId;
}