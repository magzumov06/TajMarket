using Application.Features.Order.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Commands.CreateReturnRequest;

public class CreateReturnRequestCommand(int userId, CreateReturnRequestDto dto) : IRequest<Response<ReturnRequestDto>>
{
    public int UserId { get; } = userId;
    public CreateReturnRequestDto Dto { get; } = dto;
}