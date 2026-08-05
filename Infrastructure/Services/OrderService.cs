using System.Net;
using Application.Features.Address.DTOs;
using Domain.DTOs.AddressDtos;
using Domain.DTOs.CourierDto;
using Domain.DTOs.OrderDto;
using Domain.DTOs.PaymentDtos;
using Domain.Entities.AddressEntity;
using Domain.Entities.OrderEntity;
using Domain.Entities.PaymentEntity;
using Domain.Entities.UserEntity;
using Domain.Enums;
using Domain.Filters;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Realtime;
using Infrastructure.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class OrderService(
    DataContext context,
    ICouponService couponService,
    INotificationService notificationService,
    IHubContext<CourierHub> hubContext,
    IOptions<ShippingSetting> shippingSettings,
    ILogger<OrderService> logger) : IOrderService
{
    #region CreateOrder

    public async Task<Response<string>> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        logger.LogInformation(
            "CreateOrderAsync started: user={UserId}, shippingAddress={ShippingAddressId}, coupon={CouponCode}",
            userId,
            dto.ShippingAddressId,
            dto.CouponCode);

        try
        {
            var (pricing, errorStatus, errorMessage) =
                await BuildPricingAsync(userId, dto.ShippingAddressId, dto.CouponCode);

            if (pricing == null)
                return new Response<string>(errorStatus, errorMessage!);


            await using var transaction =
                await context.Database.BeginTransactionAsync();


            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                SubTotal = pricing.SubTotal,
                ShippingCost = pricing.ShippingCost,
                DiscountAmount = pricing.DiscountAmount,
                TotalAmount = pricing.TotalAmount,
                Note = dto.Note,
                ShippingAddressId = dto.ShippingAddressId,

                OrderItems = pricing.Cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.Product.Name,
                    ProductImageUrl =
                        i.Product.Images.FirstOrDefault(im => im.IsMain)?.Url
                        ?? i.Product.Images.FirstOrDefault()?.Url,

                    Quantity = i.Quantity,
                    UnitPrice = UnitPrice(i),
                    TotalPrice = UnitPrice(i) * i.Quantity
                }).ToList()
            };


            context.Orders.Add(order);


            foreach (var item in pricing.Cart.Items)
            {
                if (item.ProductVariant != null)
                    item.ProductVariant.StockQuantity -= item.Quantity;
                else
                    item.Product.StockQuantity -= item.Quantity;
            }


            context.Payments.Add(new Payment
            {
                Order = order,
                Amount = pricing.TotalAmount,
                Method = dto.PaymentMethod,
                Status = PaymentStatus.Pending
            });


            context.CartItems.RemoveRange(pricing.Cart.Items);


            await context.SaveChangesAsync();


            if (pricing.AppliedCouponId.HasValue)
                await couponService.IncrementUsageAsync(pricing.AppliedCouponId.Value);


            await transaction.CommitAsync();


            await notificationService.NotifyAsync(
                userId,
                "Фармоиш сабт шуд",
                $"Фармоиши шумо №{order.OrderNumber} бо маблағи {pricing.TotalAmount:0.00} сомонӣ қабул шуд");


            logger.LogInformation(
                "Order {OrderNumber} created successfully for user {UserId}",
                order.OrderNumber,
                userId);


            return new Response<string>(
                HttpStatusCode.OK,
                "Order successfully created");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "CreateOrderAsync failed for user {UserId}",
                userId);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion

    #region CalculateTotalPrice

    public async Task<Response<OrderPreviewDto>> CalculateTotalPriceAsync(int userId, CalculateTotalPriceDto dto)
    {
        try
        {
            logger.LogInformation(
                "CalculateTotalPriceAsync started for user {UserId}, shippingAddress={ShippingAddressId}",
                userId, dto.ShippingAddressId);

            var (pricing, errorStatus, errorMessage) =
                await BuildPricingAsync(userId, dto.ShippingAddressId, dto.CouponCode);

            if (pricing == null)
                return new Response<OrderPreviewDto>(errorStatus, errorMessage!);


            var items = pricing.Cart.Items.Select(i => new OrderPreviewItemDto(
                i.ProductId,
                i.Product.Name,
                i.Product.Images.FirstOrDefault(im => im.IsMain)?.Url
                    ?? i.Product.Images.FirstOrDefault()?.Url,
                i.Quantity,
                UnitPrice(i),
                UnitPrice(i) * i.Quantity)).ToList();

            var preview = new OrderPreviewDto(
                items,
                pricing.SubTotal,
                pricing.DiscountAmount,
                pricing.ShippingCost,
                pricing.TotalAmount);


            logger.LogInformation(
                "CalculateTotalPriceAsync completed for user {UserId}, total {TotalAmount}",
                userId, pricing.TotalAmount);

            return new Response<OrderPreviewDto>(preview);
        }
        catch (Exception e)
        {
            logger.LogError(e, "CalculateTotalPriceAsync failed for user {UserId}", userId);

            return new Response<OrderPreviewDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion

    #region CancelOrder

    public async Task<Response<string>> CancelOrderAsync(int userId, int orderId)
    {
        try
        {
            logger.LogInformation(
                "CancelOrderAsync started for order {OrderId} and user {UserId}",
                orderId,
                userId);


            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == userId);


            if (order == null)
            {
                logger.LogWarning(
                    "Order {OrderId} not found for user {UserId}",
                    orderId,
                    userId);

                return new Response<string>(
                    HttpStatusCode.NotFound,
                    "Фармоиш ёфт нашуд");
            }


            if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            {
                logger.LogWarning(
                    "Order {OrderId} cannot be cancelled because status is {Status}",
                    orderId,
                    order.Status);

                return new Response<string>(
                    HttpStatusCode.BadRequest,
                    "Ин фармоиш дигар бекор карда намешавад");
            }


            await using var transaction =
                await context.Database.BeginTransactionAsync();


            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var variant = await context.ProductVariants
                        .FirstOrDefaultAsync(v =>
                            v.Id == item.ProductVariantId.Value);


                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;
                    }
                    else
                    {
                        var product = await context.Products
                            .FirstOrDefaultAsync(p =>
                                p.Id == item.ProductId);

                        if (product != null)
                            product.StockQuantity += item.Quantity;
                    }
                }
                else
                {
                    var product = await context.Products
                        .FirstOrDefaultAsync(p =>
                            p.Id == item.ProductId);

                    if (product != null)
                        product.StockQuantity += item.Quantity;
                }
            }


            order.Status = OrderStatus.Cancelled;


            if (order.Payment != null)
            {
                if (order.Payment.Status == PaymentStatus.Completed)
                {
                    order.Payment.Status = PaymentStatus.Refunded;
                }
                else if (order.Payment.Status == PaymentStatus.Pending)
                {
                    order.Payment.Status = PaymentStatus.Failed;
                }
            }


            await ReleaseCourierIfAssignedAsync(order);


            await context.SaveChangesAsync();

            await transaction.CommitAsync();


            await notificationService.NotifyAsync(
                userId,
                "Фармоиш бекор шуд",
                $"Фармоиши шумо №{order.OrderNumber} бекор карда шуд");


            logger.LogInformation(
                "Order {OrderId} cancelled successfully",
                orderId);


            return new Response<string>(
                HttpStatusCode.OK,
                "Order successfully cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "CancelOrderAsync failed for order {OrderId} and user {UserId}",
                orderId,
                userId);


            return new Response<string>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion
    
    #region GetOrderList

    public async Task<PaginationResponse<List<OrderListDto>>> GetOrderListAsync(int userId, OrderFilter filter)
    {
        try
        {
            logger.LogInformation(
                "Retrieving order list for user {UserId} with filter {@Filter}",
                userId, filter);


            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .AsQueryable();

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

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


            logger.LogInformation(
                "Retrieved {OrderCount} orders for user {UserId}, total {TotalCount}",
                orders.Count,
                userId,
                totalCount);


            return new PaginationResponse<List<OrderListDto>>(
                orders.Select(ToListDto).ToList(),
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "GetOrderListAsync failed for user {UserId}",
                userId);


            return new PaginationResponse<List<OrderListDto>>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion
    
    #region GetOrderDetails

    public async Task<Response<OrderDetailDto>> GetOrderDetailAsync(
        int orderId,
        int userId)
    {
        try
        {
            logger.LogInformation(
                "Retrieving order detail for order {OrderId} and user {UserId}",
                orderId,
                userId);


            var order = await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == userId);


            if (order == null)
            {
                logger.LogWarning(
                    "Order not found {OrderId} for user {UserId}",
                    orderId,
                    userId);


                return new Response<OrderDetailDto>(
                    HttpStatusCode.NotFound,
                    "Фармоиш ёфт нашуд");
            }


            logger.LogInformation(
                "Order detail retrieved successfully {OrderId}",
                orderId);


            return new Response<OrderDetailDto>(
                ToDetailDto(order));
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "GetOrderDetailAsync failed for order {OrderId}",
                orderId);


            return new Response<OrderDetailDto>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion

    #region GetBySellerId

    public async Task<PaginationResponse<List<OrderListDto>>> GetBySellerIdAsync(int sellerUserId, OrderFilter filter)
    {
        try
        {
            logger.LogInformation(
                "Retrieving orders for seller user {SellerUserId} with filter {@Filter}",
                sellerUserId, filter);


            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == sellerUserId);


            if (sellerProfile == null)
            {
                logger.LogWarning(
                    "Seller profile not found for user {SellerUserId}",
                    sellerUserId);


                return new PaginationResponse<List<OrderListDto>>(
                    HttpStatusCode.NotFound,
                    "Seller profile not found");
            }


            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Product.SellerProfileId == sellerProfile.Id))
                .AsQueryable();

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

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


            logger.LogInformation(
                "Retrieved {OrderCount} orders for seller user {SellerUserId}, total {TotalCount}",
                orders.Count,
                sellerUserId,
                totalCount);


            return new PaginationResponse<List<OrderListDto>>(
                orders.Select(ToListDto).ToList(),
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "GetBySellerIdAsync failed for seller user {SellerUserId}",
                sellerUserId);


            return new PaginationResponse<List<OrderListDto>>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion
    
    #region UpdateStatus

    public async Task<Response<OrderDetailDto>> UpdateStatusAsync(
        int sellerUserId,
        int orderId,
        UpdateOrderStatusDto dto)
    {
        try
        {
            logger.LogInformation(
                "Updating status for order {OrderId} to {Status} by seller user {SellerUserId}",
                orderId,
                dto.Status,
                sellerUserId);


            var sellerProfile = await context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == sellerUserId);

            if (sellerProfile == null)
            {
                logger.LogWarning("Seller profile not found for user {SellerUserId}", sellerUserId);

                return new Response<OrderDetailDto>(
                    HttpStatusCode.NotFound,
                    "Профили фурӯшанда ёфт нашуд");
            }


            var order = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);


            if (order == null)
            {
                logger.LogWarning(
                    "Order not found {OrderId}",
                    orderId);


                return new Response<OrderDetailDto>(
                    HttpStatusCode.NotFound,
                    "Фармоиш ёфт нашуд");
            }


            var ownsOrder = order.OrderItems.Any(oi =>
                oi.Product.SellerProfileId == sellerProfile.Id);

            if (!ownsOrder)
            {
                logger.LogWarning(
                    "Seller user {SellerUserId} tried to update order {OrderId} they do not own",
                    sellerUserId, orderId);

                return new Response<OrderDetailDto>(
                    HttpStatusCode.Forbidden,
                    "Шумо ба ин фармоиш дастрасӣ надоред");
            }


            if (order.Status is OrderStatus.Cancelled or OrderStatus.Returned)
            {
                logger.LogWarning(
                    "Order {OrderId} status cannot be changed because current status is {Status}",
                    orderId,
                    order.Status);


                return new Response<OrderDetailDto>(
                    HttpStatusCode.Conflict,
                    "Ҳолати ин фармоиш дигар тағйирнопазир аст");
            }


            order.Status = dto.Status;


            if (dto.Status == OrderStatus.Delivered &&
                order.Payment is { Status: PaymentStatus.Pending })
            {
                order.Payment.Status = PaymentStatus.Completed;
                order.Payment.PaidAt = DateTime.UtcNow;
            }


            if (dto.Status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Returned)
            {
                await ReleaseCourierIfAssignedAsync(order);
            }


            await context.SaveChangesAsync();


            await notificationService.NotifyAsync(
                order.UserId,
                "Ҳолати фармоиш тағйир ёфт",
                $"Фармоиши №{order.OrderNumber} ҳоло дар ҳолати «{StatusLabel(dto.Status)}» аст");


            logger.LogInformation(
                "Order {OrderId} status updated successfully to {Status}",
                orderId,
                dto.Status);


            return new Response<OrderDetailDto>(
                ToDetailDto(order));
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "UpdateStatusAsync failed for order {OrderId}",
                orderId);


            return new Response<OrderDetailDto>(
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    #endregion

    #region CompleteOrder

    public async Task<Response<OrderDetailDto>> CompleteOrderAsync(int userId, int orderId)
    {
        try
        {
            logger.LogInformation(
                "CompleteOrderAsync started for order {OrderId} by user {UserId}", orderId, userId);

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.Courier)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                logger.LogWarning("Order not found {OrderId} for user {UserId}", orderId, userId);

                return new Response<OrderDetailDto>(HttpStatusCode.NotFound, "Фармоиш ёфт нашуд");
            }


            if (order.Status is not (OrderStatus.Shipped or OrderStatus.Delivered))
            {
                logger.LogWarning(
                    "Order {OrderId} cannot be completed, current status {Status}", orderId, order.Status);

                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest,
                    "Фармоиш ҳанӯз фиристода нашудааст, тасдиқи қабул имконнопазир аст");
            }


            if (order.CustomerConfirmedAt.HasValue)
            {
                logger.LogWarning("Order {OrderId} already confirmed by customer", orderId);

                return new Response<OrderDetailDto>(HttpStatusCode.Conflict,
                    "Шумо аллакай қабули ин фармоишро тасдиқ кардаед");
            }


            order.CustomerConfirmedAt = DateTime.UtcNow;

            if (order.Status != OrderStatus.Delivered)
            {
                order.Status = OrderStatus.Delivered;

                if (order.Payment is { Status: PaymentStatus.Pending })
                {
                    order.Payment.Status = PaymentStatus.Completed;
                    order.Payment.PaidAt = DateTime.UtcNow;
                }
            }


            await ReleaseCourierIfAssignedAsync(order);


            await context.SaveChangesAsync();


            await notificationService.NotifyAsync(
                order.UserId,
                "Фармоиш анҷом ёфт",
                $"Шумо қабули фармоиши №{order.OrderNumber}-ро тасдиқ кардед. Ташаккур!");


            logger.LogInformation(
                "Order {OrderId} completed/confirmed by customer {UserId}", orderId, userId);

            return new Response<OrderDetailDto>(ToDetailDto(order));
        }
        catch (Exception e)
        {
            logger.LogError(e, "CompleteOrderAsync failed for order {OrderId}", orderId);

            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    #endregion


    // ---------------- Ёрирасонҳои дохилӣ ----------------

    private class PricingResult
    {
        public required Domain.Entities.CartEntity.Cart Cart { get; init; }
        public required Address Address { get; init; }
        public required decimal SubTotal { get; init; }
        public required decimal DiscountAmount { get; init; }
        public int? AppliedCouponId { get; init; }
        public required decimal ShippingCost { get; init; }
        public decimal TotalAmount => SubTotal - DiscountAmount + ShippingCost;
    }

    private async Task<(PricingResult? Result, HttpStatusCode ErrorStatus, string? ErrorMessage)> BuildPricingAsync(
        int userId, int shippingAddressId, string? couponCode)
    {
        var address = await context.Addresses
            .FirstOrDefaultAsync(a =>
                a.Id == shippingAddressId &&
                a.UserId == userId);

        if (address == null)
        {
            logger.LogWarning(
                "Shipping address not found {ShippingAddressId} for user {UserId}",
                shippingAddressId, userId);

            return (null, HttpStatusCode.BadRequest, "Address not found");
        }


        var cart = await context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.Items.Count == 0)
        {
            logger.LogWarning("Cart is empty for user {UserId}", userId);

            return (null, HttpStatusCode.BadRequest, "Cart is empty");
        }


        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
            {
                logger.LogWarning("Product {ProductId} is inactive", item.ProductId);

                return (null, HttpStatusCode.BadRequest, $"Маҳсулоти '{item.Product.Name}' дигар дастрас нест");
            }

            var availableStock = item.ProductVariant?.StockQuantity ?? item.Product.StockQuantity;

            if (item.Quantity > availableStock)
            {
                logger.LogWarning("Not enough stock for product {ProductId}", item.ProductId);

                return (null, HttpStatusCode.BadRequest,
                    $"Барои '{item.Product.Name}' танҳо {availableStock} дона дар анбор мондааст");
            }
        }


        var subTotal = cart.Items.Sum(i => UnitPrice(i) * i.Quantity);

        decimal discountAmount = 0;
        int? appliedCouponId = null;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var validation = await couponService.ValidateAsync(couponCode, subTotal);

            if (!validation.Success)
            {
                logger.LogWarning("Invalid coupon {CouponCode}", couponCode);

                return (null, HttpStatusCode.BadRequest, validation.Message ?? "Купон нодуруст аст");
            }

            discountAmount = Math.Min(validation.Data!.DiscountAmount, subTotal);
            appliedCouponId = validation.Data.CouponId;
        }


        var shippingCost = CalculateShippingCost(address);

        var result = new PricingResult
        {
            Cart = cart,
            Address = address,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            AppliedCouponId = appliedCouponId,
            ShippingCost = shippingCost
        };

        return (result, HttpStatusCode.OK, null);
    }

    private decimal CalculateShippingCost(Address address)
    {
        var settings = shippingSettings.Value;

        var isSameCity = string.Equals(
            address.City?.Trim(),
            settings.WarehouseCity?.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (isSameCity)
        {
            logger.LogInformation(
                "Shipping cost: same city ({City}), flat rate {Rate}",
                address.City, settings.SameCityFlatRate);

            return settings.SameCityFlatRate;
        }


        if (address.Latitude.HasValue && address.Longitude.HasValue)
        {
            var distanceKm = DistanceKm(
                settings.WarehouseLatitude, settings.WarehouseLongitude,
                address.Latitude.Value, address.Longitude.Value);

            var calculated = Math.Round((decimal)distanceKm * settings.PricePerKm, 2);
            var finalCost = Math.Max(calculated, settings.MinOtherCityRate);

            logger.LogInformation(
                "Shipping cost: other city ({City}), distance {Distance} km, cost {Cost}",
                address.City, distanceKm, finalCost);

            return finalCost;
        }


        logger.LogInformation(
            "Shipping cost: other city ({City}) without coordinates, fallback rate {Rate}",
            address.City, settings.MinOtherCityRate);

        return settings.MinOtherCityRate;
    }

    private async Task ReleaseCourierIfAssignedAsync(Order order)
    {
        if (!order.CourierId.HasValue)
            return;

        var courier = await context.Couriers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == order.CourierId.Value);

        if (courier == null || courier.Status == CourierStatus.Offline)
            return;

        courier.Status = CourierStatus.Available;

        logger.LogInformation(
            "Courier {CourierId} released back to Available after order {OrderId} reached a terminal status",
            courier.Id, order.Id);

        await BroadcastCourierUpdateAsync(courier);
    }

    private async Task BroadcastCourierUpdateAsync(Courier courier)
    {
        var payload = new CourierLiveUpdateDto(
            courier.Id,
            courier.User?.FullName ?? string.Empty,
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


    private static decimal UnitPrice(
        Domain.Entities.CartEntity.CartItem item) =>
        (item.Product.DiscountPrice ?? item.Product.Price)
        + (item.ProductVariant?.ExtraPrice ?? 0);


    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";


    private static string StatusLabel(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Дар интизорӣ",
        OrderStatus.Confirmed => "Тасдиқшуда",
        OrderStatus.Processing => "Дар ҳоли тайёркунӣ",
        OrderStatus.Shipped => "Фиристода шуд",
        OrderStatus.Delivered => "Расонида шуд",
        OrderStatus.Cancelled => "Бекоршуда",
        OrderStatus.Returned => "Баргардонидашуда",
        _ => status.ToString()
    };


    private static OrderListDto ToListDto(Order o) => new(
        o.Id,
        o.OrderNumber,
        o.OrderDate,
        o.Status,
        o.TotalAmount,
        o.OrderItems.Count
    );


    private static OrderDetailDto ToDetailDto(Order o) => new(
        o.Id,
        o.OrderNumber,
        o.OrderDate,
        o.Status,
        o.SubTotal,
        o.ShippingCost,
        o.DiscountAmount,
        o.TotalAmount,

        new AddressDto(
            o.ShippingAddress.Id,
            o.ShippingAddress.FullName,
            o.ShippingAddress.PhoneNumber,
            o.ShippingAddress.Country,
            o.ShippingAddress.City,
            o.ShippingAddress.Street,
            o.ShippingAddress.PostalCode,
            o.ShippingAddress.IsDefault,
            o.ShippingAddress.Latitude,     // <-- нав
            o.ShippingAddress.Longitude),

        o.OrderItems.Select(oi => new OrderItemDto(
            oi.ProductId,
            oi.ProductName,
            oi.ProductImageUrl,
            oi.Quantity,
            oi.UnitPrice,
            oi.TotalPrice)).ToList(),

        o.Payment == null
            ? null
            : new PaymentDto(
                o.Payment.Id,
                o.Payment.Amount,
                o.Payment.Method,
                o.Payment.Status,
                o.Payment.PaidAt),

        o.Courier == null
            ? null
            : new CourierInfoDto(
                o.Courier.Id,
                o.Courier.User.FullName,
                o.Courier.User.PhoneNumber,
                o.Courier.Status),

        o.CustomerConfirmedAt
    );
}