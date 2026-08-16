using Domain.Responses;
using MediatR;
using Application.Features.Order.Dtos;

namespace Application.Features.Order.Commands.CompleteReturn;

public class CompleteReturnCommand(int returnRequestId) : IRequest<Response<ReturnRequestDto>>
{
    public int ReturnRequestId { get; } = returnRequestId;
}