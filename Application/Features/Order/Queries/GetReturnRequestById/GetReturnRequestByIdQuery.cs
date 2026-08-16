using Application.Features.Order.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.GetReturnRequestById;

public class GetReturnRequestByIdQuery(int returnRequestId) : IRequest<Response<ReturnRequestDto>>
{
    public int ReturnRequestId { get; } = returnRequestId;
}