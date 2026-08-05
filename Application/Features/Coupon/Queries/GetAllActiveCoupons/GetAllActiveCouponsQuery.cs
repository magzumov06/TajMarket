using Application.Features.Coupon.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Coupon.Queries.GetAllActiveCoupons;

public class GetAllActiveCouponsQuery : IRequest<Response<List<CouponDto>>>;