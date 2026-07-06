using Domain.DTOs.CouponDto;
using Domain.Resposes;

namespace Infrastructure.Interfaces;

public record CouponValidationResult(int CouponId, decimal DiscountAmount);

public interface ICouponService
{
    Task<Response<List<CouponDto>>> GetAllActiveAsync();
    Task<Response<CouponDto>> CreateAsync(CreateCouponDto dto);
    Task<Response<string>> DeactivateAsync(string code);
    Task<Response<CouponValidationResult>> ValidateAsync(string code, decimal orderAmount);
    Task IncrementUsageAsync(int couponId);
}