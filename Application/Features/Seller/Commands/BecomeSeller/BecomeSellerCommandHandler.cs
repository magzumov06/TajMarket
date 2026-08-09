using System.Net;
using Application.Common.Interfaces;
using Application.Features.Seller.DTOs;
using Domain.Entities.UserEntity;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Seller.Commands.BecomeSeller;

public class BecomeSellerCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    ILogger<BecomeSellerCommandHandler> logger)
    : IRequestHandler<BecomeSellerCommand, Response<SellerProfileDto>>
{
    private const string SellerRole = "Seller";

    public async Task<Response<SellerProfileDto>> Handle(BecomeSellerCommand request, CancellationToken cancellationToken)
    {
        var (userId, dto) = (request.UserId, request.Dto);

        try
        {
            logger.LogInformation("User {UserId} trying to become seller with store {StoreName}", userId, dto.StoreName);

            var userExists = await identityService.UserExistsAsync(userId, cancellationToken);

            if (!userExists)
            {
                logger.LogWarning("User not found {UserId}", userId);
                return new Response<SellerProfileDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }

            var alreadySeller = await context.SellerProfiles.AnyAsync(sp => sp.UserId == userId, cancellationToken);

            if (alreadySeller)
            {
                logger.LogWarning("User {UserId} already has seller profile", userId);
                return new Response<SellerProfileDto>(HttpStatusCode.Conflict, "Шумо аллакай Seller ҳастед");
            }

            var storeNameTaken = await context.SellerProfiles.AnyAsync(sp => sp.StoreName == dto.StoreName, cancellationToken);

            if (storeNameTaken)
            {
                logger.LogWarning("Store name already exists {StoreName}", dto.StoreName);
                return new Response<SellerProfileDto>(HttpStatusCode.Conflict, "Ин номи мағоза аллакай истифода шудааст");
            }

            var profile = new SellerProfile
            {
                UserId = userId,
                StoreName = dto.StoreName,
                StoreDescription = dto.StoreDescription,
                IsVerified = false,
                Rating = 0,
                RegisteredAt = DateTime.UtcNow
            };

            context.SellerProfiles.Add(profile);
            await context.SaveChangesAsync(cancellationToken);

            if (!await identityService.IsInRoleAsync(userId, SellerRole, cancellationToken))
            {
                var (succeeded, _) = await identityService.AddToRoleAsync(userId, SellerRole, cancellationToken);
                if (succeeded)
                    logger.LogInformation("Seller role added to user {UserId}", userId);
            }

            logger.LogInformation("User {UserId} successfully became seller with profile {SellerProfileId}", userId, profile.Id);

            return new Response<SellerProfileDto>(
                SellerMapper.ToDto(profile, 0),
                "Шумо ҳоло Seller ҳастед. Пас аз тасдиқи админ, мағозаи шумо verified мешавад");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while user {UserId} becoming seller", userId);
            return new Response<SellerProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}