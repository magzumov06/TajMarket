using System.Net;
using Application.Common.Interfaces;
using Domain.DTOs.CourierDto;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetAvailableCouriers;

public class GetAvailableCouriersQueryHandler(
    IApplicationDbContext context,
    ILogger<GetAvailableCouriersQueryHandler> logger)
    : IRequestHandler<GetAvailableCouriersQuery, Response<List<CourierDto>>>
{
    public async Task<Response<List<CourierDto>>> Handle(GetAvailableCouriersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving available couriers");

            var couriers = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.Status == CourierStatus.Available)
                .OrderByDescending(c => c.LastLocationUpdate)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {CourierCount} available couriers", couriers.Count);

            return new Response<List<CourierDto>>(couriers.Select(c => CourierMapper.ToDto(c, c.User)).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetAvailableCouriersAsync failed");
            return new Response<List<CourierDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}