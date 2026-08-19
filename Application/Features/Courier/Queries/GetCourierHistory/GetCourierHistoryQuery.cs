using Application.Features.Courier.DTOs;
using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetCourierHistory;

public class GetCourierHistoryQuery(int userId, CourierHistoryFilter filter) : IRequest<PaginationResponse<List<CourierOrderDto>>>
{
    public int UserId { get; } = userId;
    public CourierHistoryFilter Filter { get; } = filter;
}