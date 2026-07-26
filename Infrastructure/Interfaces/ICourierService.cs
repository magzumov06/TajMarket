using Domain.DTOs.CourierDto;
using Domain.Filters;
using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface ICourierService
{
    Task<Response<CourierDto>> CreateCourierAsync(CreateCourierDto dto);
    Task<Response<CourierDto>> UpdateCourierAsync(int courierId, UpdateCourierDto dto);
    Task<Response<string>> DeleteCourierAsync(int courierId);
    Task<Response<CourierDto>> GetByIdAsync(int courierId);
    Task<Response<List<CourierDto>>> GetAllAsync();

    Task<Response<string>> UpdateLocationAsync(int userId, UpdateCourierLocationDto dto);
    Task<Response<string>> ChangeStatusAsync(int userId, UpdateCourierStatusDto dto);
    Task<Response<CourierLocationDto>> GetLocationAsync(int courierId);
    Task<Response<List<CourierMapDto>>> GetMapAsync();

    Task<Response<List<CourierDto>>> GetAvailableCouriersAsync();
    Task<Response<string>> AssignCourierToOrderAsync(int orderId, int courierId);

    Task<Response<List<CourierOrderDto>>> GetMyOrdersAsync(int userId);
    Task<PaginationResponse<List<CourierOrderDto>>> GetHistoryAsync(int userId, CourierHistoryFilter filter);
}