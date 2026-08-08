using System.Net;
using Application.Common.Interfaces;
using Domain.DTOs.CourierDto;
using Domain.Enums;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courier.Commands.CreateCourier;

public class CreateCourierCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    ILogger<CreateCourierCommandHandler> logger)
    : IRequestHandler<CreateCourierCommand, Response<CourierDto>>
{
    private const string CourierRole = "Courier";

    public async Task<Response<CourierDto>> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        try
        {
            logger.LogInformation("CreateCourierAsync started for user {UserId}", dto.UserId);

            var user = await identityService.FindUserByIdAsync(dto.UserId, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", dto.UserId);
                return new Response<CourierDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }

            var alreadyCourier = await context.Couriers.AnyAsync(c => c.UserId == dto.UserId, cancellationToken);

            if (alreadyCourier)
            {
                logger.LogWarning("User {UserId} already has a courier profile", dto.UserId);
                return new Response<CourierDto>(HttpStatusCode.Conflict, "Ин корбар аллакай курьер аст");
            }

            var courier = new Domain.Entities.UserEntity.Courier
            {
                UserId = dto.UserId,
                VehicleType = dto.VehicleType,
                Status = CourierStatus.Offline,
                CreatedAt = DateTime.UtcNow
            };

            context.Couriers.Add(courier);
            await context.SaveChangesAsync(cancellationToken);

            if (!await identityService.IsInRoleAsync(dto.UserId, CourierRole, cancellationToken))
            {
                await identityService.AddToRoleAsync(dto.UserId, CourierRole, cancellationToken);
                logger.LogInformation("Courier role added to user {UserId}", dto.UserId);
            }

            logger.LogInformation("Courier {CourierId} created for user {UserId}", courier.Id, dto.UserId);

            return new Response<CourierDto>(CourierMapper.ToDto(courier, user));
        }
        catch (Exception e)
        {
            logger.LogError(e, "CreateCourierAsync failed for user {UserId}", dto.UserId);
            return new Response<CourierDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}