using Application.Features.Order.DTOs;
using Domain.DTOs.PaymentDtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Payment.Commands.ProcessPayment;

public class ProcessPaymentCommand(int userId, ProcessPaymentDto dto) : IRequest<Response<OrderDetailDto>>
{
    public int UserId { get; } = userId;
    public ProcessPaymentDto Dto { get; } = dto;
}