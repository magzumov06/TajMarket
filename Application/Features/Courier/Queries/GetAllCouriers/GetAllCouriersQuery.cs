using Application.Features.Courier.DTOs;
using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetAllCouriers;

public class GetAllCouriersQuery : IRequest<Response<List<CourierDto>>>;