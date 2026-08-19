using System.Net;
using Application.Common.Interfaces;
using Application.Features.Courier.DTOs;
using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetAllCouriers;

public class GetAllCouriersQueryHandler(
    IApplicationDbContext context,
    ILogger<GetAllCouriersQueryHandler> logger)
    : IRequestHandler<GetAllCouriersQuery, Response<List<CourierDto>>>
{
    public async Task<Response<List<CourierDto>>> Handle(GetAllCouriersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving all couriers");

            var couriers = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {CourierCount} couriers", couriers.Count);

            return new Response<List<CourierDto>>(couriers.Select(c => CourierMapper.ToDto(c, c.User)).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetAllAsync failed");
            return new Response<List<CourierDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}