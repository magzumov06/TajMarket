using System.Net;
using Domain.DTOs.AddressDtos;
using Domain.DTOs.OrderDto;
using Domain.DTOs.PaymentDtos;
using Domain.Entities.OrderEntity;
using Domain.Entities.PaymentEntity;
using Domain.Enums;
using Domain.Responses;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class OrderService(
    DataContext context,
    ICouponService couponService,
    INotificationService notificationService) : IOrderService
{
    #region CreateOrder
    public async Task<Response<string>> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        Log.ForContext<OrderService>().Information("CreateOrderAsync started: user={UserId}, shippingAddress={ShippingAddressId}, coupon={CouponCode}", userId, dto.ShippingAddressId, dto.CouponCode);
        try
        {
            var address = await context.Addresses.FirstOrDefaultAsync(a => a.Id == dto.ShippingAddressId && a.UserId == userId);
            if (address == null)
                return new Response<string>(HttpStatusCode.BadRequest,"Address not found");

            var cart = await context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .Include(c => c.Items).ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (cart == null || cart.Items.Count == 0)
                return new Response<string>(HttpStatusCode.BadRequest,"Cart is empty");

            foreach (var item in cart.Items)
            {
                if(!item.Product.IsActive)
                    return new Response<string>(HttpStatusCode.BadRequest,$"Маҳсулоти '{item.Product.Name}' дигар дастрас нест");
                
                var availableStock = item.ProductVariant?.StockQuantity ?? item.Product.StockQuantity;
                if (item.Quantity > availableStock)
                    return new Response<string>(HttpStatusCode.BadRequest,
                        $"Барои '{item.Product.Name}' танҳо {availableStock} дона дар анбор мондааст");
            }
            var subTotal = cart.Items.Sum(i => UnitPrice(i) * i.Quantity);
            
            // --- Купон (агар вуҷуд дошта бошад) ---
            decimal discountAmount = 0;
            int? appliedCouponId = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var validation = await couponService.ValidateAsync(dto.CouponCode, subTotal);
                if (!validation.Success)
                    return new Response<string>(HttpStatusCode.BadRequest, validation.Message ?? "Купон нодуруст аст" );
                
                discountAmount = Math.Min(validation.Data!.DiscountAmount, subTotal);
                appliedCouponId = validation.Data.CouponId;
            }

            const decimal shippingCost = 0;
            var totalAmount = subTotal - discountAmount + shippingCost;
            
            await using var  transaction = await context.Database.BeginTransactionAsync();

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Note = dto.Note,
                ShippingAddressId = dto.ShippingAddressId,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.Product.Name,
                    ProductImageUrl = i.Product.Images.FirstOrDefault(im => im.IsMain)?.Url
                                      ?? i.Product.Images.FirstOrDefault()?.Url,
                    Quantity = i.Quantity,
                    UnitPrice = UnitPrice(i),
                    TotalPrice = UnitPrice(i) * i.Quantity
                }).ToList()
            };
            
            context.Orders.Add(order);
            
            // --- Кам кардани миқдори анбор ---
            foreach (var item in cart.Items)
            {
                if (item.ProductVariant != null)
                    item.ProductVariant.StockQuantity -= item.Quantity;
                else
                    item.Product.StockQuantity -= item.Quantity;
            }
            
            context.Payments.Add(new Payment
            {
                Order = order,
                Amount = totalAmount,
                Method = dto.PaymentMethod,
                Status = PaymentStatus.Pending
            });
            
            context.CartItems.RemoveRange(cart.Items);

            await context.SaveChangesAsync();

            if (appliedCouponId.HasValue)
                await couponService.IncrementUsageAsync(appliedCouponId.Value);
            
            await transaction.CommitAsync();

            await notificationService.NotifyAsync(userId, "Фармоиш сабт шуд",
                $"Фармоиши шумо №{order.OrderNumber} бо маблағи {totalAmount:0.00} сомонӣ қабул шуд");
            
            Log.Information("Order {OrderNumber} created successfully for user {UserId}", order.OrderNumber, userId);
            return new Response<string>(HttpStatusCode.OK,"Order successfully created");
            
        }
        catch (Exception e)
        {
            Log.Error(e, "CreateOrderAsync failed for user {UserId}", userId);
            return new Response<string>(HttpStatusCode.InternalServerError,"Internal server error");
        }
    }
    #endregion
    
    #region CancelOrder
    public async Task<Response<string>> CancelOrderAsync(int userId, int orderId)
    {
        try
        {
            Log.Information("CancelOrderAsync started for order {OrderId} and user {UserId}", orderId, userId);
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            
            if (order == null)
                return new Response<string>(HttpStatusCode.NotFound,"Фармоиш ёфт нашуд");
            
            if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
                return new Response<string>(HttpStatusCode.BadRequest,"Ин фармоиш дигар бекор карда намешавад");
            
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.ProductVariantId.Value);
                    if (variant != null)
                        variant.StockQuantity += item.Quantity;
                    else
                    {
                        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                        if (product != null)
                            product.StockQuantity += item.Quantity;
                    }
                }
                else
                {
                    var product = await context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                    if (product != null)
                        product.StockQuantity += item.Quantity;
                }
            }

            order.Status = OrderStatus.Cancelled;
            if (order.Payment != null)
            {
                if (order.Payment.Status == PaymentStatus.Completed)
                    order.Payment.Status = PaymentStatus.Refunded;
                else if (order.Payment.Status == PaymentStatus.Pending)
                    order.Payment.Status = PaymentStatus.Failed;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await notificationService.NotifyAsync(userId, "Фармоиш бекор шуд",
                $"Фармоиши шумо №{order.OrderNumber} бекор карда шуд");

            Log.Information("Order {OrderId} cancelled successfully", orderId);
            return new Response<string>(HttpStatusCode.OK, "Order successfully cancelled");
        }
        catch (Exception e)
        {
            Log.Error(e, "CancelOrderAsync failed for order {OrderId} and user {UserId}", orderId, userId);
            return new Response<string>(HttpStatusCode.InternalServerError,"Internal server error");
        } 
    }
    #endregion

    #region GetOrderList
    public async Task<Response<List<OrderListDto>>> GetOrderListAsync(int userId)
    {
        try
        {
            Log.Information("Retrieving order list for user {UserId}", userId);
            var orders = await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            Log.Information("Retrieved {OrderCount} orders for user {UserId}", orders.Count, userId);
            return  new Response<List<OrderListDto>>(orders.Select(ToListDto).ToList());
        }
        catch (Exception e)
        {
            Log.Error(e, "GetOrderListAsync failed for user {UserId}", userId);
            return new Response<List<OrderListDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
    #endregion

    #region GetOrderDetails
    public async Task<Response<OrderDetailDto>> GetOrderDetailAsync(int orderId, int userId)
    {
        try
        {
            Log.Information("Retrieving order detail for order {OrderId} and user {UserId}", orderId, userId);
            var order = await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                Log.Warning("Order not found: {OrderId} for user {UserId}", orderId, userId);
                return new Response<OrderDetailDto>(HttpStatusCode.BadRequest, "Фармоиш ёфт нашуд");
            }

            Log.Information("Order detail retrieved successfully for order {OrderId}", orderId);
            return new Response<OrderDetailDto>(ToDetailDto(order));
        }
        catch (Exception e)
        {
            Log.Error(e, "GetOrderDetailAsync failed for order {OrderId} and user {UserId}", orderId, userId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError,"Internal server error");
        }
    }
    #endregion

    #region GetBySellerId

    public async Task<Response<List<OrderListDto>>> GetBySellerIdAsync(int sellerUserId)
    {
        try
        {
            Log.Information("Retrieving orders for seller user {SellerUserId}", sellerUserId);
            var sellerProfile = await context.SellerProfiles.FirstOrDefaultAsync(sp => sp.UserId == sellerUserId);
            if (sellerProfile == null)
                return new Response<List<OrderListDto>>(HttpStatusCode.NotFound, "Seller profile not found");

            var orders = await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => oi.Product.SellerProfileId == sellerProfile.Id))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            Log.Information("Retrieved {OrderCount} orders for seller user {SellerUserId}", orders.Count, sellerUserId);
            return new Response<List<OrderListDto>>(orders.Select(ToListDto).ToList());
        }
        catch (Exception e)
        {
            Log.Error(e, "GetBySellerIdAsync failed for seller user {SellerUserId}", sellerUserId);
            return new Response<List<OrderListDto>>(HttpStatusCode.InternalServerError,"Internal server error");
        }
    }

    #endregion

    #region UpdateStatus

    public async Task<Response<OrderDetailDto>> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        try
        {
            Log.Information("Updating status for order {OrderId} to {Status}", orderId, dto.Status);
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return new Response<OrderDetailDto>(HttpStatusCode.NotFound,"Фармоиш ёфт нашуд");

            if (order.Status is OrderStatus.Cancelled or OrderStatus.Returned)
                return new Response<OrderDetailDto>(HttpStatusCode.Conflict,"Ҳолати ин фармоиш дигар тағйирнопазир аст");

            order.Status = dto.Status;

            if (dto.Status == OrderStatus.Delivered && order.Payment is { Status: PaymentStatus.Pending })
            {
                order.Payment.Status = PaymentStatus.Completed;
                order.Payment.PaidAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            await notificationService.NotifyAsync(order.UserId, "Ҳолати фармоиш тағйир ёфт",
                $"Фармоиши №{order.OrderNumber} ҳоло дар ҳолати «{StatusLabel(dto.Status)}» аст");

            Log.Information("Order {OrderId} status updated to {Status}", orderId, dto.Status);
            return new Response<OrderDetailDto>(ToDetailDto(order));
        }
        catch (Exception e)
        {
            Log.Error(e, "UpdateStatusAsync failed for order {OrderId}", orderId);
            return new Response<OrderDetailDto>(HttpStatusCode.InternalServerError,"Internal server error");
        }
    }

    #endregion
    
    private static decimal UnitPrice(Domain.Entities.CartEntity.CartItem item) =>
        (item.Product.DiscountPrice ?? item.Product.Price) + (item.ProductVariant?.ExtraPrice ?? 0);
    
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
        o.Id, o.OrderNumber, o.OrderDate, o.Status, o.TotalAmount, o.OrderItems.Count
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
            o.ShippingAddress.Id, o.ShippingAddress.FullName, o.ShippingAddress.PhoneNumber,
            o.ShippingAddress.Country, o.ShippingAddress.City, o.ShippingAddress.Street,
            o.ShippingAddress.PostalCode, o.ShippingAddress.IsDefault),
        o.OrderItems.Select(oi => new OrderItemDto(
            oi.ProductId, oi.ProductName, oi.ProductImageUrl, oi.Quantity, oi.UnitPrice, oi.TotalPrice)).ToList(),
        o.Payment == null ? null : new PaymentDto(o.Payment.Id, o.Payment.Amount, o.Payment.Method, o.Payment.Status, o.Payment.PaidAt)
    );
}