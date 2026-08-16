using Application.Features.Order.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.GetMyReturnRequests;

public class GetMyReturnRequestsQuery(int userId) : IRequest<Response<List<ReturnRequestDto>>>
{
    public int UserId { get; } = userId;
}