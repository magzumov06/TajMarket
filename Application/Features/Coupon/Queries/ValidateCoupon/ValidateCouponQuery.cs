using Application.Features.Coupon.Dtos;
using Domain.Responses;
using MediatR;

namespace Application.Features.Coupon.Queries.ValidateCoupon;

public class ValidateCouponQuery(string code, decimal orderAmount) : IRequest<Response<CouponValidationResult>>
{
    public string Code { get; } = code;
    public decimal OrderAmount { get; } = orderAmount;
}