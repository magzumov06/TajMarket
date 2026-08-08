using Application.Features.Payment.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Payment.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQuery(int userId, int orderId) : IRequest<Response<PaymentDto>>
{
    public int UserId { get; } = userId;
    public int OrderId { get; } = orderId;
}