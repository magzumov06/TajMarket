// Application/Features/Seller/Queries/GetMySellerProfile/GetMySellerProfileQueryHandler.cs
using System.Net;
using Application.Common.Interfaces;
using Application.Features.Seller.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Seller.Queries.GetMySellerProfile;

public class GetMySellerProfileQueryHandler(
    IApplicationDbContext context,
    ILogger<GetMySellerProfileQueryHandler> logger)
    : IRequestHandler<GetMySellerProfileQuery, Response<SellerProfileDto>>
{
    public async Task<Response<SellerProfileDto>> Handle(GetMySellerProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;

        try
        {
            logger.LogInformation("Getting seller profile for user {UserId}", userId);

            var profile = await context.SellerProfiles
                .Include(sp => sp.Products)
                .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

            if (profile == null)
            {
                logger.LogWarning("Seller profile not found for user {UserId}", userId);
                return new Response<SellerProfileDto>(HttpStatusCode.NotFound, "Профили Seller ёфт нашуд");
            }

            return new Response<SellerProfileDto>(
                SellerMapper.ToDto(profile, profile.Products?.Count(p => p.IsActive) ?? 0));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting seller profile for user {UserId}", userId);
            return new Response<SellerProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}