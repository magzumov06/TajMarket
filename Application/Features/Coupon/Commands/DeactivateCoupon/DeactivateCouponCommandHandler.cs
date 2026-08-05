using System.Net;
using Application.Common.Interfaces;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Coupon.Commands.DeactivateCoupon;

public class DeactivateCouponCommandHandler(
    IApplicationDbContext context,
    ILogger<DeactivateCouponCommandHandler> logger)
    : IRequestHandler<DeactivateCouponCommand, Response<string>>
{
    public async Task<Response<string>> Handle(DeactivateCouponCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code;

        try
        {
            logger.LogInformation("Deactivating coupon {CouponCode}", code);

            if (string.IsNullOrWhiteSpace(code))
            {
                logger.LogWarning("Coupon code is empty");
                return new Response<string>(HttpStatusCode.BadRequest, "Coupon code is required");
            }

            var coupon = await context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpper(), cancellationToken);

            if (coupon == null)
            {
                logger.LogWarning("Coupon not found {CouponCode}", code);
                return new Response<string>(HttpStatusCode.NotFound, "Купон ёфт нашуд");
            }

            coupon.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Coupon deactivated successfully {CouponId}", coupon.Id);

            return new Response<string>(HttpStatusCode.OK, "Купон ғайрифаъол карда шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deactivating coupon {CouponCode}", code);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}