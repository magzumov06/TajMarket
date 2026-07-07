using Domain.DTOs.CouponDto;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public record CouponValidationResult(int CouponId, decimal DiscountAmount);

public interface ICouponService
{
    Task<Response<CouponDto>> CreateAsync(CreateCouponDto dto);
    Task<Response<string>> DeactivateAsync(string code);
    Task<Response<CouponValidationResult>> ValidateAsync(string code, decimal orderAmount);
    Task<Response<List<CouponDto>>> GetAllActiveAsync();
    Task IncrementUsageAsync(int couponId);
}