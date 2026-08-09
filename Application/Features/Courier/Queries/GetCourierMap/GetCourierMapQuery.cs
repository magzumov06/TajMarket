using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.Courier.Queries.GetCourierMap;

public class GetCourierMapQuery : IRequest<Response<List<CourierMapDto>>>;