using System.Net;
using Application.Common.Interfaces;
using Application.Features.Seller.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Seller.Queries.GetSellerById;

public class GetSellerByIdQueryHandler(
    IApplicationDbContext context,
    ILogger<GetSellerByIdQueryHandler> logger)
    : IRequestHandler<GetSellerByIdQuery, Response<SellerProfileDto>>
{
    public async Task<Response<SellerProfileDto>> Handle(GetSellerByIdQuery request, CancellationToken cancellationToken)
    {
        var sellerProfileId = request.SellerProfileId;

        try
        {
            logger.LogInformation("Getting seller profile {SellerProfileId}", sellerProfileId);

            var profile = await context.SellerProfiles
                .Include(sp => sp.Products)
                .FirstOrDefaultAsync(sp => sp.Id == sellerProfileId, cancellationToken);

            if (profile == null)
            {
                logger.LogWarning("Seller profile not found {SellerProfileId}", sellerProfileId);
                return new Response<SellerProfileDto>(HttpStatusCode.NotFound, "Мағоза ёфт нашуд");
            }

            return new Response<SellerProfileDto>(
                SellerMapper.ToDto(profile, profile.Products?.Count(p => p.IsActive) ?? 0));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting seller profile {SellerProfileId}", sellerProfileId);
            return new Response<SellerProfileDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}