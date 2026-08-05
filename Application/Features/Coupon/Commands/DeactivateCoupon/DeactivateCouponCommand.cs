using Domain.Responses;
using MediatR;

namespace Application.Features.Coupon.Commands.DeactivateCoupon;

public class DeactivateCouponCommand(string code) : IRequest<Response<string>>
{
    public string Code { get; } = code;
}