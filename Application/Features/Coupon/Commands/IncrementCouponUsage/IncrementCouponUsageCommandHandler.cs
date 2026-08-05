using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Coupon.Commands.IncrementCouponUsage;


public class IncrementCouponUsageCommandHandler(
    IApplicationDbContext context,
    ILogger<IncrementCouponUsageCommandHandler> logger)
    : IRequestHandler<IncrementCouponUsageCommand>
{
    public async Task Handle(IncrementCouponUsageCommand request, CancellationToken cancellationToken)
    {
        var couponId = request.CouponId;

        try
        {
            logger.LogInformation("Incrementing coupon usage {CouponId}", couponId);

            var coupon = await context.Coupons
                .FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);

            if (coupon == null)
            {
                logger.LogWarning("Coupon not found {CouponId}", couponId);
                return;
            }

            coupon.UsedCount++;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Coupon usage incremented successfully {CouponId}", couponId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error incrementing coupon usage {CouponId}", couponId);
        }
    }
}