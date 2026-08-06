using Application.Features.Cart.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Cart.Queries.GetCartItems;

public class GetCartItemsQuery(int userId) : IRequest<Response<CartDto>>
{
    public int UserId { get; } = userId;
}