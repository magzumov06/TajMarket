using Domain.DTOs.CouponDto;
using Domain.Respoces;

namespace Infrastructure.Interfaces;

public record CouponValidationResult(int CouponId, decimal DiscountAmount);

public interface ICouponService
{
    Task<List<CouponDto>> GetAllActiveAsync();
    Task<Responce<string>> CreateAsync(CreateCouponDto dto);
    Task<Responce<string>> DeactivateAsync(string code);
    Task<Responce<CouponValidationResult>> ValidateAsync(string code, decimal orderAmount);
    Task IncrementUsageAsync(int couponId);
}