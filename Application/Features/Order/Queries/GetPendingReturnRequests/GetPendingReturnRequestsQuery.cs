using Application.Features.Order.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Order.Queries.GetPendingReturnRequests;

public class GetPendingReturnRequestsQuery : IRequest<Response<List<ReturnRequestDto>>>;