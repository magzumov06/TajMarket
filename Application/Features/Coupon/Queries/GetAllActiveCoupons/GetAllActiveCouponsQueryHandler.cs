using System.Net;
using Application.Common.Interfaces;
using Application.Features.Coupon.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Coupon.Queries.GetAllActiveCoupons;

public class GetAllActiveCouponsQueryHandler(
    IApplicationDbContext context,
    ILogger<GetAllActiveCouponsQueryHandler> logger)
    : IRequestHandler<GetAllActiveCouponsQuery, Response<List<CouponDto>>>
{
    public async Task<Response<List<CouponDto>>> Handle(GetAllActiveCouponsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Retrieving all active coupons");

            var coupons = await context.Coupons
                .AsNoTracking()
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.UtcNow)
                .OrderByDescending(c => c.ExpiryDate)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {CouponCount} active coupons", coupons.Count);

            return new Response<List<CouponDto>>(coupons.Select(CouponMapper.ToDto).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving active coupons");
            return new Response<List<CouponDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}