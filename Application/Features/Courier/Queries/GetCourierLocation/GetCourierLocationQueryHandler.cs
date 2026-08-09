using System.Net;
using Application.Common.Interfaces;
using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetCourierLocation;

public class GetCourierLocationQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCourierLocationQueryHandler> logger)
    : IRequestHandler<GetCourierLocationQuery, Response<CourierLocationDto>>
{
    public async Task<Response<CourierLocationDto>> Handle(GetCourierLocationQuery request, CancellationToken cancellationToken)
    {
        var courierId = request.CourierId;

        try
        {
            logger.LogInformation("Retrieving location for courier {CourierId}", courierId);

            var courier = await context.Couriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);
                return new Response<CourierLocationDto>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }

            return new Response<CourierLocationDto>(new CourierLocationDto(
                courier.Latitude, courier.Longitude, courier.LastLocationUpdate));
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetLocationAsync failed for courier {CourierId}", courierId);
            return new Response<CourierLocationDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}