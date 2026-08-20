using System.Net;
using Application.Common.Interfaces;
using Application.Features.Courier.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Queries.GetCourierById;

public class GetCourierByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetCourierByIdQueryHandler> logger)
    : IRequestHandler<GetCourierByIdQuery, Response<CourierDto>>
{
    public async Task<Response<CourierDto>> Handle(GetCourierByIdQuery request, CancellationToken cancellationToken)
    {
        var courierId = request.CourierId;

        try
        {
            logger.LogInformation("Retrieving courier {CourierId}", courierId);

            var courier = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);
                return new Response<CourierDto>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }

            return new Response<CourierDto>(CourierMapper.ToDto(courier, courier.User));
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetByIdAsync failed for courier {CourierId}", courierId);
            return new Response<CourierDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}