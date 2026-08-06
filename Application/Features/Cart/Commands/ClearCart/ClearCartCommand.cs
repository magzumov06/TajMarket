using Domain.Responses;
using MediatR;

namespace Application.Features.Cart.Commands.ClearCart;

public class ClearCartCommand(int userId) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
}