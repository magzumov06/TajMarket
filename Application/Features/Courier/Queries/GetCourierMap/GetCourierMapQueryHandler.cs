using System.Net;
using Application.Common.Interfaces;
using Application.Features.Courier.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetCourierMap;

public class GetCourierMapQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCourierMapQueryHandler> logger)
    : IRequestHandler<GetCourierMapQuery, Response<List<CourierMapDto>>>
{
    public async Task<Response<List<CourierMapDto>>> Handle(GetCourierMapQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving couriers for map");

            var couriers = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.Latitude != null && c.Longitude != null)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {CourierCount} couriers for map", couriers.Count);

            var items = couriers.Select(c => new CourierMapDto(
                c.Id, c.User.FullName, c.Latitude, c.Longitude, c.Status.ToString())).ToList();

            return new Response<List<CourierMapDto>>(items);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetMapAsync failed");
            return new Response<List<CourierMapDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}