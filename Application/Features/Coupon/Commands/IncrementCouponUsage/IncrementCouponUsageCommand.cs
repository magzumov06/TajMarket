using MediatR;

namespace Application.Features.Coupon.Commands.IncrementCouponUsage;


public class IncrementCouponUsageCommand(int couponId) : IRequest
{
    public int CouponId { get; } = couponId;
}