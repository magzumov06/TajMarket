using Application.Features.Courier.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetAvailableCouriers;

public class GetAvailableCouriersQuery : IRequest<Response<List<CourierDto>>>;