using System.Net;
using Application.Features.Courier;
using Domain.DTOs.CourierDto;
using Domain.Entities.OrderEntity;
using Domain.Entities.UserEntity;
using Domain.Enums;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Realtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class CourierService(
    DataContext context,
    UserManager<User> userManager,
    INotificationService notificationService,
    IHubContext<CourierHub> hubContext,
    ILogger<CourierService> logger) : ICourierService
{
    private const string CourierRole = "Courier";

    #region CreateCourier

    public async Task<Response<CourierDto>> CreateCourierAsync(CreateCourierDto dto)
    {
        try
        {
            logger.LogInformation("CreateCourierAsync started for user {UserId}", dto.UserId);

            var user = await userManager.FindByIdAsync(dto.UserId.ToString());

            if (user == null)
            {
                logger.LogWarning("User not found {UserId}", dto.UserId);

                return new Response<CourierDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");
            }


            var alreadyCourier = await context.Couriers
                .AnyAsync(c => c.UserId == dto.UserId);

            if (alreadyCourier)
            {
                logger.LogWarning("User {UserId} already has a courier profile", dto.UserId);

                return new Response<CourierDto>(HttpStatusCode.Conflict, "Ин корбар аллакай курьер аст");
            }


            var courier = new Courier
            {
                UserId = dto.UserId,
                VehicleType = dto.VehicleType,
                Status = CourierStatus.Offline,
                CreatedAt = DateTime.UtcNow
            };

            context.Couriers.Add(courier);

            await context.SaveChangesAsync();


            if (!await userManager.IsInRoleAsync(user, CourierRole))
            {
                await userManager.AddToRoleAsync(user, CourierRole);

                logger.LogInformation("Courier role added to user {UserId}", dto.UserId);
            }


            logger.LogInformation("Courier {CourierId} created for user {UserId}", courier.Id, dto.UserId);

            return new Response<CourierDto>(ToDto(courier, user));
        }
        catch (Exception e)
        {
            logger.LogError(e, "CreateCourierAsync failed for user {UserId}", dto.UserId);

            return new Response<CourierDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region UpdateCourier

    public async Task<Response<CourierDto>> UpdateCourierAsync(int courierId, UpdateCourierDto dto)
    {
        try
        {
            logger.LogInformation("UpdateCourierAsync started for courier {CourierId}", courierId);

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);

                return new Response<CourierDto>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }


            courier.VehicleType = dto.VehicleType;

            await context.SaveChangesAsync();


            logger.LogInformation("Courier {CourierId} updated successfully", courierId);

            return new Response<CourierDto>(ToDto(courier, courier.User));
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateCourierAsync failed for courier {CourierId}", courierId);

            return new Response<CourierDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region DeleteCourier

    public async Task<Response<string>> DeleteCourierAsync(int courierId)
    {
        try
        {
            logger.LogInformation("DeleteCourierAsync started for courier {CourierId}", courierId);

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);

                return new Response<string>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }


            var hasActiveOrders = await context.Orders.AnyAsync(o =>
                o.CourierId == courierId &&
                o.Status != OrderStatus.Delivered &&
                o.Status != OrderStatus.Cancelled &&
                o.Status != OrderStatus.Returned);

            if (hasActiveOrders)
            {
                logger.LogWarning("Courier {CourierId} has active orders and cannot be deleted", courierId);

                return new Response<string>(HttpStatusCode.Conflict,
                    "Курьер фармоишҳои фаъол дорад, аввал онҳоро анҷом диҳед");
            }


            context.Couriers.Remove(courier);

            await context.SaveChangesAsync();


            if (courier.User != null && await userManager.IsInRoleAsync(courier.User, CourierRole))
            {
                await userManager.RemoveFromRoleAsync(courier.User, CourierRole);
            }


            logger.LogInformation("Courier {CourierId} deleted successfully", courierId);

            return new Response<string>(HttpStatusCode.OK, "Курьер бо муваффақият нест карда шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "DeleteCourierAsync failed for courier {CourierId}", courierId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region GetById

    public async Task<Response<CourierDto>> GetByIdAsync(int courierId)
    {
        try
        {
            logger.LogInformation("Retrieving courier {CourierId}", courierId);

            var courier = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);

                return new Response<CourierDto>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }


            return new Response<CourierDto>(ToDto(courier, courier.User));
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetByIdAsync failed for courier {CourierId}", courierId);

            return new Response<CourierDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region GetAll

    public async Task<Response<List<CourierDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Retrieving all couriers");

            var couriers = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


            logger.LogInformation("Retrieved {CourierCount} couriers", couriers.Count);

            return new Response<List<CourierDto>>(couriers.Select(c => ToDto(c, c.User)).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetAllAsync failed");

            return new Response<List<CourierDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region UpdateLocation

    public async Task<Response<string>> UpdateLocationAsync(int userId, UpdateCourierLocationDto dto)
    {
        try
        {
            logger.LogInformation("UpdateLocationAsync started for user {UserId}", userId);

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);

                return new Response<string>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }


            courier.Latitude = dto.Latitude;
            courier.Longitude = dto.Longitude;
            courier.LastLocationUpdate = DateTime.UtcNow;

            await context.SaveChangesAsync();


            logger.LogInformation("Location updated for courier {CourierId}: {Lat},{Lng}",
                courier.Id, dto.Latitude, dto.Longitude);

            await BroadcastCourierUpdateAsync(courier);

            return new Response<string>(HttpStatusCode.OK, "Координата бо муваффақият нав шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "UpdateLocationAsync failed for user {UserId}", userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region ChangeStatus

    public async Task<Response<string>> ChangeStatusAsync(int userId, UpdateCourierStatusDto dto)
    {
        try
        {
            logger.LogInformation("ChangeStatusAsync started for user {UserId} to {Status}", userId, dto.Status);

            if (dto.Status == CourierStatus.Assigned)
            {
                logger.LogWarning("User {UserId} tried to manually set Assigned status", userId);

                return new Response<string>(HttpStatusCode.BadRequest,
                    "Ҳолати 'Assigned' автоматикӣ ҳангоми таъини фармоиш гузошта мешавад");
            }


            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);

                return new Response<string>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }


            courier.Status = dto.Status;

            await context.SaveChangesAsync();


            logger.LogInformation("Courier {CourierId} status changed to {Status}", courier.Id, dto.Status);

            await BroadcastCourierUpdateAsync(courier);

            return new Response<string>(HttpStatusCode.OK, "Ҳолати курьер бо муваффақият иваз шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "ChangeStatusAsync failed for user {UserId}", userId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region GetLocation

    public async Task<Response<CourierLocationDto>> GetLocationAsync(int courierId)
    {
        try
        {
            logger.LogInformation("Retrieving location for courier {CourierId}", courierId);

            var courier = await context.Couriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courierId);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);

                return new Response<CourierLocationDto>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }


            return new Response<CourierLocationDto>(new CourierLocationDto(
                courier.Latitude,
                courier.Longitude,
                courier.LastLocationUpdate));
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetLocationAsync failed for courier {CourierId}", courierId);

            return new Response<CourierLocationDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region GetMap

    public async Task<Response<List<CourierMapDto>>> GetMapAsync()
    {
        try
        {
            logger.LogInformation("Retrieving couriers for map");

            var couriers = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.Latitude != null && c.Longitude != null)
                .ToListAsync();


            logger.LogInformation("Retrieved {CourierCount} couriers for map", couriers.Count);

            var items = couriers.Select(c => new CourierMapDto(
                c.Id,
                c.User.FullName,
                c.Latitude,
                c.Longitude,
                c.Status.ToString())).ToList();

            return new Response<List<CourierMapDto>>(items);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetMapAsync failed");

            return new Response<List<CourierMapDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region GetAvailableCouriers

    public async Task<Response<List<CourierDto>>> GetAvailableCouriersAsync()
    {
        try
        {
            logger.LogInformation("Retrieving available couriers");

            var couriers = await context.Couriers
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.Status == CourierStatus.Available)
                .OrderByDescending(c => c.LastLocationUpdate)
                .ToListAsync();


            logger.LogInformation("Retrieved {CourierCount} available couriers", couriers.Count);

            return new Response<List<CourierDto>>(couriers.Select(c => ToDto(c, c.User)).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetAvailableCouriersAsync failed");

            return new Response<List<CourierDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region AssignCourierToOrder (дастӣ — Admin/Seller курьерро худ интихоб мекунад)

    public async Task<Response<string>> AssignCourierToOrderAsync(int orderId, int courierId)
    {
        try
        {
            logger.LogInformation(
                "AssignCourierToOrderAsync started for order {OrderId} and courier {CourierId}",
                orderId, courierId);

            var orderCheck = await ValidateOrderForAssignmentAsync(orderId);
            if (orderCheck.Error != null)
                return orderCheck.Error;

            var courier = await context.Couriers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == courierId);

            if (courier == null)
            {
                logger.LogWarning("Courier not found {CourierId}", courierId);

                return new Response<string>(HttpStatusCode.NotFound, "Курьер ёфт нашуд");
            }

            if (courier.Status != CourierStatus.Available)
            {
                logger.LogWarning(
                    "Courier {CourierId} is not available, current status {Status}",
                    courierId, courier.Status);

                return new Response<string>(HttpStatusCode.BadRequest, "Курьер дар айни замон дастрас нест");
            }

            await PerformAssignmentAsync(orderCheck.Order!, courier);

            logger.LogInformation("Courier {CourierId} assigned to order {OrderId}", courierId, orderId);

            return new Response<string>(HttpStatusCode.OK, "Курьер бо муваффақият ба фармоиш таъин шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "AssignCourierToOrderAsync failed for order {OrderId} and courier {CourierId}",
                orderId, courierId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region AutoAssignCourierToOrder (худкор — наздиктарин курьери Available)

    public async Task<Response<string>> AutoAssignCourierToOrderAsync(int orderId)
    {
        try
        {
            logger.LogInformation("AutoAssignCourierToOrderAsync started for order {OrderId}", orderId);

            var orderCheck = await ValidateOrderForAssignmentAsync(orderId, includeAddress: true);
            if (orderCheck.Error != null)
                return orderCheck.Error;

            var order = orderCheck.Order!;

            var availableCouriers = await context.Couriers
                .Include(c => c.User)
                .Where(c => c.Status == CourierStatus.Available)
                .ToListAsync();

            if (availableCouriers.Count == 0)
            {
                logger.LogWarning("No available couriers for order {OrderId}", orderId);

                return new Response<string>(HttpStatusCode.BadRequest, "Дар айни замон курьери дастрас нест");
            }


            Courier nearest;

            var destLat = order.ShippingAddress?.Latitude;
            var destLng = order.ShippingAddress?.Longitude;

            var couriersWithLocation = availableCouriers
                .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
                .ToList();

            if (destLat.HasValue && destLng.HasValue && couriersWithLocation.Count > 0)
            {
                // Наздиктарин курьер нисбат ба суроғаи фиристониш (формулаи Haversine)
                nearest = couriersWithLocation
                    .OrderBy(c => DistanceKm(
                        destLat.Value, destLng.Value,
                        c.Latitude!.Value, c.Longitude!.Value))
                    .First();

                logger.LogInformation(
                    "Nearest courier {CourierId} selected for order {OrderId} by distance",
                    nearest.Id, orderId);
            }
            else
            {
                // Фоллбек: суроға ё координатаи курьерҳо нест — курьери навтарин фаъол интихоб мешавад
                nearest = availableCouriers
                    .OrderByDescending(c => c.LastLocationUpdate ?? DateTime.MinValue)
                    .First();

                logger.LogInformation(
                    "Fallback courier {CourierId} selected for order {OrderId} (no coordinates available)",
                    nearest.Id, orderId);
            }


            await PerformAssignmentAsync(order, nearest);

            logger.LogInformation(
                "Courier {CourierId} auto-assigned to order {OrderId}", nearest.Id, orderId);

            return new Response<string>(HttpStatusCode.OK, "Наздиктарин курьер бо муваффақият таъин шуд");
        }
        catch (Exception e)
        {
            logger.LogError(e, "AutoAssignCourierToOrderAsync failed for order {OrderId}", orderId);

            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region GetMyOrders

    public async Task<Response<List<CourierOrderDto>>> GetMyOrdersAsync(int userId)
    {
        try
        {
            logger.LogInformation("Retrieving active orders for courier user {UserId}", userId);

            var courier = await context.Couriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);

                return new Response<List<CourierOrderDto>>(HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }


            var orders = await context.Orders
                .AsNoTracking()
                .Include(o => o.Buyer)
                .Include(o => o.ShippingAddress)
                .Where(o => o.CourierId == courier.Id &&
                            o.Status != OrderStatus.Delivered &&
                            o.Status != OrderStatus.Cancelled &&
                            o.Status != OrderStatus.Returned)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();


            logger.LogInformation(
                "Retrieved {OrderCount} active orders for courier user {UserId}",
                orders.Count, userId);

            return new Response<List<CourierOrderDto>>(orders.Select(ToOrderDto).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetMyOrdersAsync failed for user {UserId}", userId);

            return new Response<List<CourierOrderDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region GetHistory

    public async Task<PaginationResponse<List<CourierOrderDto>>> GetHistoryAsync(
        int userId,
        CourierHistoryFilter filter)
    {
        try
        {
            logger.LogInformation(
                "Retrieving order history for courier user {UserId} with filter {@Filter}",
                userId, filter);

            var courier = await context.Couriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (courier == null)
            {
                logger.LogWarning("Courier profile not found for user {UserId}", userId);

                return new PaginationResponse<List<CourierOrderDto>>(
                    HttpStatusCode.NotFound, "Профили курьер ёфт нашуд");
            }


            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.Buyer)
                .Include(o => o.ShippingAddress)
                .Where(o => o.CourierId == courier.Id &&
                            (o.Status == OrderStatus.Delivered ||
                             o.Status == OrderStatus.Cancelled ||
                             o.Status == OrderStatus.Returned))
                .AsQueryable();


            query = filter.SortBy switch
            {
                "date_asc" => query.OrderBy(o => o.OrderDate),
                _ => query.OrderByDescending(o => o.OrderDate)
            };


            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = orders.Select(ToOrderDto).ToList();


            logger.LogInformation(
                "Retrieved {ItemCount} history orders for courier user {UserId}, total {TotalCount}",
                items.Count, userId, totalCount);

            return new PaginationResponse<List<CourierOrderDto>>(
                items, totalCount, filter.PageNumber, filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetHistoryAsync failed for user {UserId}", userId);

            return new PaginationResponse<List<CourierOrderDto>>(
                HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion


    // ---------------- Ёрирасонҳои дохилӣ ----------------

    private async Task<(Order? Order, Response<string>? Error)> ValidateOrderForAssignmentAsync(
        int orderId, bool includeAddress = false)
    {
        var query = context.Orders.AsQueryable();

        if (includeAddress)
            query = query.Include(o => o.ShippingAddress);

        var order = await query.FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            logger.LogWarning("Order not found {OrderId}", orderId);

            return (null, new Response<string>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд"));
        }

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered or OrderStatus.Returned)
        {
            logger.LogWarning(
                "Order {OrderId} cannot be assigned because status is {Status}", orderId, order.Status);

            return (null, new Response<string>(HttpStatusCode.BadRequest,
                "Ба ин фармоиш курьер таъин кардан мумкин нест"));
        }

        if (order.CourierId.HasValue)
        {
            logger.LogWarning("Order {OrderId} already has a courier assigned", orderId);

            return (null, new Response<string>(HttpStatusCode.Conflict,
                "Ба ин фармоиш аллакай курьер таъин шудааст"));
        }

        return (order, null);
    }

    private async Task PerformAssignmentAsync(Order order, Courier courier)
    {
        order.CourierId = courier.Id;
        courier.Status = CourierStatus.Assigned;

        await context.SaveChangesAsync();

        await notificationService.NotifyAsync(
            courier.UserId,
            "Фармоиши нав",
            $"Ба шумо фармоиши №{order.OrderNumber} таъин карда шуд");

        await BroadcastCourierUpdateAsync(courier);
    }

    private async Task BroadcastCourierUpdateAsync(Courier courier)
    {
        var fullName = courier.User?.FullName
            ?? (await userManager.FindByIdAsync(courier.UserId.ToString()))?.FullName
            ?? string.Empty;

        var payload = new CourierLiveUpdateDto(
            courier.Id,
            fullName,
            courier.Latitude,
            courier.Longitude,
            courier.Status.ToString());

        await hubContext.Clients.All.SendAsync("CourierUpdated", payload);
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;


    private static CourierDto ToDto(Courier c, User user) => new(
        c.Id,
        c.UserId,
        user.FullName,
        user.PhoneNumber,
        c.Status,
        c.VehicleType,
        c.Latitude,
        c.Longitude,
        c.LastLocationUpdate,
        c.CreatedAt);


    private static CourierOrderDto ToOrderDto(Order o) => new(
        o.Id,
        o.OrderNumber,
        o.OrderDate,
        o.Status,
        o.TotalAmount,
        o.ShippingAddress.City,
        o.ShippingAddress.Street,
        o.Buyer.FullName,
        o.Buyer.PhoneNumber);
}