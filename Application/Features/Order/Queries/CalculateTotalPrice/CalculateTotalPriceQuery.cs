using Application.Features.Order.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.CalculateTotalPrice;

public class CalculateTotalPriceQuery(int userId, CalculateTotalPriceDto dto) : IRequest<Response<OrderPreviewDto>>
{
    public int UserId { get; } = userId;
    public CalculateTotalPriceDto Dto { get; } = dto;
}