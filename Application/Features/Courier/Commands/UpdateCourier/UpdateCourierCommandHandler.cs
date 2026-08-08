using System.Net;
using Application.Common.Interfaces;
using Domain.DTOs.CourierDto;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Commands.UpdateCourier;

public class UpdateCourierCommandHandler(
    IApplicationDbContext context,
    ILogger<UpdateCourierCommandHandler> logger)
    : IRequestHandler<UpdateCourierCommand, Response<CourierDto>>
{
    public async Task<Response<CourierDto>> Handle(UpdateCourierCommand request, CancellationToken cancellationToken)
    {
        var (courierId, dto) = (request.CourierId, request.Dto);

        try
        {
            logger.LogInformation("UpdateCourierAsync started for courier {CourierId}", courierId);

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);
                return new Response<CourierDto>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }

            courier.VehicleType = dto.VehicleType;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Courier {CourierId} updated successfully", courierId);

            return new Response<CourierDto>(CourierMapper.ToDto(courier, courier.User));
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateCourierAsync failed for courier {CourierId}", courierId);
            return new Response<CourierDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}