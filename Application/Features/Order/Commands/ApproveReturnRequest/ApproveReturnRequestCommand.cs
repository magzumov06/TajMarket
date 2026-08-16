using Application.Features.Order.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.ApproveReturnRequest;

public class ApproveReturnRequestCommand(int returnRequestId, ResolveReturnRequestDto dto) : IRequest<Response<ReturnRequestDto>>
{
    public int ReturnRequestId { get; } = returnRequestId;
    public ResolveReturnRequestDto Dto { get; } = dto;
}