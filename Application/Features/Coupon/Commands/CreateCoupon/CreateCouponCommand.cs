using Application.Features.Coupon.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Coupon.Commands.CreateCoupon;

public class CreateCouponCommand(CreateCouponDto dto) : IRequest<Response<CouponDto>>
{
    public CreateCouponDto Dto { get; } = dto;
}